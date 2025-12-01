namespace MinimalEndpoints.Attributes;

/// <summary>
/// Applies an endpoint filter to the endpoint
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class EndpointFilterAttribute(Type filterType) : Attribute
{
    /// <summary>
    /// The type of the filter to apply (must implement IEndpointFilter)
    /// </summary>
    public Type FilterType { get; } = filterType;
}
