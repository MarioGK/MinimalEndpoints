using Microsoft.AspNetCore.Routing;

namespace TerraScale.MinimalEndpoints.Groups;

/// <summary>
/// Interface for endpoint groups. Implementations may override Name to
/// provide a friendly display name for OpenAPI grouping, define a RoutePrefix,
/// and configure the group via RouteGroupBuilder.
/// </summary>
public interface IEndpointGroup
{
    /// <summary>
    /// Friendly name used for grouping. Default implementations may just use the
    /// class name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The route prefix for this group. Defaults to "/" in the base class.
    /// </summary>
    string RoutePrefix { get; }

    /// <summary>
    /// Configures the route group builder (e.g. adding authentication, filters, etc.).
    /// </summary>
    /// <param name="builder">The route group builder.</param>
    void Configure(RouteGroupBuilder builder);
}