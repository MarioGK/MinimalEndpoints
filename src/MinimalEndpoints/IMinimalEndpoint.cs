namespace MinimalEndpoints;

/// <summary>
/// Interface that all minimal endpoint classes must implement
/// </summary>
public interface IMinimalEndpoint
{
    /// <summary>
    /// Gets the group name for this endpoint (used for OpenAPI grouping)
    /// </summary>
    string? GroupName { get; }
    
    /// <summary>
    /// Gets the tags for this endpoint (used for OpenAPI categorization)
    /// </summary>
    string[]? Tags { get; }
}
