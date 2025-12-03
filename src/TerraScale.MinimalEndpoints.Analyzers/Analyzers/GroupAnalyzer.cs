using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TerraScale.MinimalEndpoints.Analyzers.Helpers;
using TerraScale.MinimalEndpoints.Analyzers.Models;

namespace TerraScale.MinimalEndpoints.Analyzers.Analyzers;

internal static class GroupAnalyzer
{
    public static GroupModel AnalyzeGroup(INamedTypeSymbol groupSymbol)
    {
        var model = new GroupModel
        {
            ClassNamespace = groupSymbol.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty,
            ClassName = groupSymbol.Name,
            TypeFullName = groupSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
        };

        // Attributes
        model.HasAuthorize = groupSymbol.GetAttributes().Any(a => a.AttributeClass?.Name.Contains("Authorize") == true);
        model.HasAllowAnonymous = groupSymbol.GetAttributes().Any(a => a.AttributeClass?.Name.Contains("AllowAnonymous") == true);

        var authorizeAttribute = groupSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name.Contains("Authorize") == true);

        if (authorizeAttribute != null)
        {
            model.Policy = authorizeAttribute.NamedArguments
                .FirstOrDefault(a => a.Key == "Policy").Value.Value?.ToString();
            model.Roles = authorizeAttribute.NamedArguments
                .FirstOrDefault(a => a.Key == "Roles").Value.Value?.ToString();
            model.AuthenticationSchemes = authorizeAttribute.NamedArguments
                .FirstOrDefault(a => a.Key == "AuthenticationSchemes").Value.Value?.ToString();
        }

        // EndpointGroupName
        var groupNameAttr = groupSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "EndpointGroupNameAttribute");

        if (groupNameAttr != null && groupNameAttr.ConstructorArguments.Length > 0)
        {
            model.EndpointGroupNameAttributeValue = groupNameAttr.ConstructorArguments[0].Value?.ToString();
        }

        // EndpointFilter
        var filterAttrs = groupSymbol.GetAttributes()
            .Where(a => a.AttributeClass?.Name == "EndpointFilterAttribute");

        foreach (var attr in filterAttrs)
        {
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is INamedTypeSymbol typeSymbol)
            {
                model.EndpointFilters.Add(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }
        }

        // Tags from XML
        var tags = XmlDocumentationHelper.GetXmlDocumentationTags(groupSymbol);
        if (tags != null)
        {
            model.Tags.AddRange(tags);
        }

        return model;
    }
}
