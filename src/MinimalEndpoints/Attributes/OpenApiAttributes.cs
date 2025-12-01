using System;

namespace TerraScale.MinimalEndpoints.Attributes;

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
