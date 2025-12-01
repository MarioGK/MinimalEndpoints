using System;

namespace TerraScale.MinimalEndpoints.Attributes;

/// <summary>
/// Marks a class as containing Minimal API endpoints that will be automatically registered
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class MinimalEndpointsAttribute : Attribute
{
    /// <summary>
    /// Optional base route for all endpoints in this class
    /// </summary>
    public string BaseRoute { get; }

    public MinimalEndpointsAttribute(string baseRoute)
    {
        BaseRoute = baseRoute;
    }
}
