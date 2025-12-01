namespace MinimalEndpoints.Attributes;

/// <summary>
/// Specifies the group name for an endpoint class (used for OpenAPI grouping)
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class EndpointGroupNameAttribute(string groupName) : Attribute
{
    /// <summary>
    /// The group name for this endpoint
    /// </summary>
    public string GroupName { get; } = groupName;
}

/// <summary>
/// Specifies the content types that an endpoint produces
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class ProducesAttribute : Attribute
{
    /// <summary>
    /// The content types this endpoint produces
    /// </summary>
    public string[] ContentTypes { get; }

    /// <summary>
    /// The HTTP status code. Defaults to 200.
    /// </summary>
    public int StatusCode { get; set; } = 200;

    public ProducesAttribute(params string[] contentTypes)
    {
        ContentTypes = contentTypes ?? [];
    }
}

/// <summary>
/// Specifies the content types that an endpoint consumes
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ConsumesAttribute : Attribute
{
    /// <summary>
    /// The content types this endpoint consumes
    /// </summary>
    public string[] ContentTypes { get; }

    public ConsumesAttribute(params string[] contentTypes)
    {
        ContentTypes = contentTypes ?? [];
    }
}

/// <summary>
/// Specifies a response description for a status code
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class ResponseDescriptionAttribute : Attribute
{
    /// <summary>
    /// The HTTP status code
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// The description for this status code
    /// </summary>
    public string Description { get; }

    public ResponseDescriptionAttribute(int statusCode, string description)
    {
        StatusCode = statusCode;
        Description = description;
    }
}
