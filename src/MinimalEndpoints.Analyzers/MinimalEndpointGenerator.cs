using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MinimalEndpoints.Analyzers.Analyzers;
using MinimalEndpoints.Analyzers.Generators;
using MinimalEndpoints.Analyzers.Models;

namespace MinimalEndpoints.Analyzers;

[Generator]
public class MinimalEndpointGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all classes with MinimalEndpoints attribute or HTTP method attributes
        var endpointClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => EndpointAnalyzer.IsEndpointClass(s),
                transform: static (ctx, _) => EndpointAnalyzer.GetEndpointClass(ctx))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider.Combine(endpointClasses.Collect());

        // Generate the source code
        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> endpointClasses, SourceProductionContext context)
    {
        if (endpointClasses.IsDefaultOrEmpty)
            return;

        var endpointMethods = new List<EndpointMethod>();

        foreach (var classSyntax in endpointClasses)
        {
            var semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax, context.CancellationToken) as INamedTypeSymbol;
            
            if (classSymbol is null)
                continue;

            // Validate that class implements IMinimalEndpoint interface
            // Diagnostics handled by EndpointDiagnosticAnalyzer
            if (!classSymbol.AllInterfaces.Any(i => i.ToDisplayString().Contains("IMinimalEndpoint")))
            {
                continue;
            }

            // Validate single endpoint per file - count HTTP method attributes
            var httpMethodCount = classSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Count(m => m.DeclaredAccessibility == Accessibility.Public &&
                           !m.IsStatic &&
                           m.GetAttributes().Any(a => a.AttributeClass?.Name.Contains("Http") == true));

            if (httpMethodCount == 0)
            {
                continue; // No HTTP methods, skip this class
            }

            // Diagnostics handled by EndpointDiagnosticAnalyzer
            if (httpMethodCount > 1)
            {
                continue;
            }

            // Get the base route from MinimalEndpoints attribute
            var baseRoute = EndpointAnalyzer.GetBaseRoute(classSymbol);

            // Find all public methods with HTTP attributes in the class
            foreach (var member in classSymbol.GetMembers())
            {
                if (member is IMethodSymbol methodSymbol && 
                    methodSymbol.DeclaredAccessibility == Accessibility.Public &&
                    !methodSymbol.IsStatic)
                {
                    var dummyDiagnostics = new List<Diagnostic>();
                    var endpointMethod = EndpointAnalyzer.AnalyzeEndpointMethod(methodSymbol, classSyntax, semanticModel, baseRoute, dummyDiagnostics);
                    if (endpointMethod != null)
                    {
                        endpointMethods.Add(endpointMethod);
                    }
                }
            }
        }

        // Generate the endpoint registration code
        var sourceCode = EndpointRegistrationGenerator.GenerateEndpointRegistrationCode(endpointMethods);
        context.AddSource("MinimalEndpoints.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
    }
}
