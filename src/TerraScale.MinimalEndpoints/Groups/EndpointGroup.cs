using Microsoft.AspNetCore.Routing;

namespace TerraScale.MinimalEndpoints.Groups;

/// <summary>
/// Convenience base class for endpoint groups. By default the group name is
/// the class name, but derived classes can override Name to use a more
/// descriptive value.
/// </summary>
public abstract class EndpointGroup : IEndpointGroup
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract string RoutePrefix { get; }

    /// <inheritdoc />
    public abstract void Configure(RouteGroupBuilder builder);
}
