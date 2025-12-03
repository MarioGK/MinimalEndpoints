using System.Collections.Generic;

namespace TerraScale.MinimalEndpoints.Analyzers.Models;

public class GroupModel
{
    public string ClassNamespace { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string TypeFullName { get; set; } = string.Empty;

    // Attributes
    public bool HasAuthorize { get; set; }
    public bool HasAllowAnonymous { get; set; }
    public string? Policy { get; set; }
    public string? Roles { get; set; }
    public string? AuthenticationSchemes { get; set; }

    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// The value from [EndpointGroupName] attribute, if present.
    /// </summary>
    public string? EndpointGroupNameAttributeValue { get; set; }

    public List<string> EndpointFilters { get; set; } = new List<string>();
}
