using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MinimalEndpoints.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EndpointDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor AsyncRequired = new(
        "TSME001",
        "Endpoint method must be async",
        "Endpoint method '{0}' must return a Task (async). All endpoint methods must be asynchronous.",
        "TerraScale.MinimalEndpoints",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InterfaceRequired = new(
        "TSME002",
        "Endpoint class must implement IMinimalEndpoint",
        "Endpoint class '{0}' must implement the IMinimalEndpoint interface",
        "TerraScale.MinimalEndpoints",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor SingleEndpoint = new(
        "TSME003",
        "Only one endpoint per file allowed",
        "Endpoint class '{0}' contains {1} HTTP method endpoints. Only one endpoint per file is allowed.",
        "TerraScale.MinimalEndpoints",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        AsyncRequired,
        InterfaceRequired,
        SingleEndpoint
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        // Check for MinimalEndpoints attribute or Http Methods
        var hasMinimalEndpointsAttribute = namedType.GetAttributes()
            .Any(a => a.AttributeClass?.Name.Contains("MinimalEndpoints") == true);

        var httpMethods = namedType.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.DeclaredAccessibility == Accessibility.Public &&
                       !m.IsStatic &&
                       m.GetAttributes().Any(a => a.AttributeClass?.Name.Contains("Http") == true))
            .ToList();

        var hasHttpMethodAttributes = httpMethods.Any();

        if (!hasMinimalEndpointsAttribute && !hasHttpMethodAttributes)
            return;

        // TSME002: Must implement IMinimalEndpoint
        // Using string check for interface name
        if (!namedType.AllInterfaces.Any(i => i.ToDisplayString().Contains("IMinimalEndpoint")))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InterfaceRequired,
                namedType.Locations.FirstOrDefault(),
                namedType.Name));
        }

        // TSME003: Single endpoint per file
        if (httpMethods.Count > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                SingleEndpoint,
                namedType.Locations.FirstOrDefault(),
                namedType.Name,
                httpMethods.Count));
        }

        // TSME001: Async required
        foreach (var method in httpMethods)
        {
            if (!method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Contains("Task"))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    AsyncRequired,
                    method.Locations.FirstOrDefault(),
                    method.Name));
            }
        }
    }
}
