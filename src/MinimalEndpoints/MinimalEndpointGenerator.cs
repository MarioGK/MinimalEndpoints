using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MinimalEndpoints.Analyzers;
using MinimalEndpoints.Generators;
using MinimalEndpoints.Models;

namespace MinimalEndpoints;

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
        var diagnostics = new List<Diagnostic>();

        foreach (var classSyntax in endpointClasses)
        {
            var semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax, context.CancellationToken) as INamedTypeSymbol;
            
            if (classSymbol is null)
                continue;

            // Validate that class implements IMinimalEndpoint interface
            if (!classSymbol.AllInterfaces.Any(i => i.ToDisplayString().Contains("IMinimalEndpoint")))
            {
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "TSME002",
                        "Endpoint class must implement IMinimalEndpoint",
                        "Endpoint class '{0}' must implement the IMinimalEndpoint interface.",
                        "TerraScale.MinimalEndpoints",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    classSyntax.Identifier.GetLocation(),
                    classSymbol.Name);
                diagnostics.Add(diagnostic);
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

            if (httpMethodCount > 1)
            {
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "TSME003",
                        "Only one endpoint per file allowed",
                        "Endpoint class '{0}' contains {1} HTTP method endpoints. Only one endpoint per file is allowed.",
                        "TerraScale.MinimalEndpoints",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    classSyntax.Identifier.GetLocation(),
                    classSymbol.Name,
                    httpMethodCount);
                diagnostics.Add(diagnostic);
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
                    var endpointMethod = EndpointAnalyzer.AnalyzeEndpointMethod(methodSymbol, classSyntax, semanticModel, baseRoute, diagnostics);
                    if (endpointMethod != null)
                    {
                        endpointMethods.Add(endpointMethod);
                    }
                }
            }
        }

        // Report any diagnostics
        foreach (var diagnostic in diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }

        // Generate the endpoint registration code
        var sourceCode = EndpointRegistrationGenerator.GenerateEndpointRegistrationCode(endpointMethods);
        context.AddSource("MinimalEndpoints.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
    }
}
