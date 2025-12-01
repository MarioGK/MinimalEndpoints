namespace MinimalEndpoints.Attributes;

/// <summary>
/// Specifies a response description for a status code
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class ResponseDescriptionAttribute(int statusCode, string description) : Attribute
{
    /// <summary>
    /// The HTTP status code
    /// </summary>
    public int StatusCode { get; } = statusCode;

    /// <summary>
    /// The description for this status code
    /// </summary>
    public string Description { get; } = description;
}
