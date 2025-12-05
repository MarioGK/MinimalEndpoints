using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TerraScale.MinimalEndpoints.Analyzers.Analyzers;
using TerraScale.MinimalEndpoints.Analyzers.Generators;
using TerraScale.MinimalEndpoints.Analyzers.Models;

namespace TerraScale.MinimalEndpoints.Analyzers;

[Generator]
public class MinimalEndpointGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Always generate a test file to verify generator is working
        context.RegisterSourceOutput(context.CompilationProvider, static (spc, compilation) => {
            var testSource = "// Generator is working!\n// Assembly: " + compilation.AssemblyName + "\n";
            spc.AddSource("GeneratorTest.g.cs", SourceText.From(testSource, Encoding.UTF8));
        });

        // Find all classes with MinimalEndpoints attribute or HTTP method attributes
        var endpointClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => EndpointAnalyzer.IsEndpointClass(s),
                transform: static (ctx, _) => EndpointAnalyzer.GetEndpointClass(ctx))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider.Combine(endpointClasses.Collect());

        // Generate source code
        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> endpointClasses, SourceProductionContext context)
    {
        // Always generate a debug file to see if generator is running
        var debugSource = $"// Generated at {DateTime.UtcNow:O}\n// Found {endpointClasses.Length} endpoint classes\n";
        context.AddSource("Debug.g.cs", SourceText.From(debugSource, Encoding.UTF8));
        
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

            // Validate single endpoint per file - count public instance methods (one endpoint per class)
            var httpMethodCount = classSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Count(m => m.DeclaredAccessibility == Accessibility.Public &&
                           !m.IsStatic &&
                           m.MethodKind == MethodKind.Ordinary);

            if (httpMethodCount == 0)
            {
                continue; // No public handler methods, skip this class
            }

            // Diagnostics handled by EndpointDiagnosticAnalyzer
            if (httpMethodCount > 1)
            {
                continue;
            }

            // Get the base route from MinimalEndpoints attribute
            var baseRoute = EndpointAnalyzer.GetBaseRoute(classSymbol);

            // Find all public instance methods and analyze them (Analyzer will filter out non-endpoints)
            foreach (var member in classSymbol.GetMembers())
            {
                if (member is IMethodSymbol methodSymbol &&
                    methodSymbol.DeclaredAccessibility == Accessibility.Public &&
                    !methodSymbol.IsStatic &&
                    methodSymbol.MethodKind == MethodKind.Ordinary)
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

        // Analyze groups
        var groups = new List<GroupModel>();
        var groupTypeNames = endpointMethods
            .Where(m => !string.IsNullOrEmpty(m.GroupTypeFullName))
            .Select(m => m.GroupTypeFullName!)
            .Distinct()
            .ToList();

        foreach (var groupTypeName in groupTypeNames)
        {
            // Strip "global::" if present
            var lookupName = groupTypeName.StartsWith("global::")
                ? groupTypeName.Substring(8)
                : groupTypeName;

            var groupSymbol = compilation.GetTypeByMetadataName(lookupName);
            if (groupSymbol != null)
            {
                var groupModel = GroupAnalyzer.AnalyzeGroup(groupSymbol);
                groups.Add(groupModel);
            }
        }

        // Generate the endpoint registration code. Include assembly name so generated
        // registration namespace is unique per compilation, avoiding duplicate extension
        // method ambiguity when multiple assemblies generate registration helpers.
        var assemblyName = compilation.AssemblyName ?? "Generated";
        var sourceCode = EndpointRegistrationGenerator.GenerateEndpointRegistrationCode(endpointMethods, groups, assemblyName);
        context.AddSource("MinimalEndpoints.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
    }
}
