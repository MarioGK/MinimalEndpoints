using System;

namespace TerraScale.MinimalEndpoints.Attributes;

/// <summary>
/// Applies an endpoint filter to the endpoint
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class EndpointFilterAttribute : Attribute
{
    /// <summary>
    /// The type of the filter to apply (must implement IEndpointFilter)
    /// </summary>
    public Type FilterType { get; }

    public EndpointFilterAttribute(Type filterType)
    {
        FilterType = filterType;
    }
}
