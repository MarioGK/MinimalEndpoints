
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace TerraScale.MinimalEndpoints;

/// <summary>
/// Base class for minimal endpoints with default implementation for GroupName and Tags
/// </summary>
public abstract class BaseMinimalApiEndpoint : IMinimalEndpoint
{
    /// <inheritdoc/>
    public virtual string? GroupName => null;

    /// <inheritdoc/>
    public virtual string[]? Tags => null;


    /// <summary>
    /// Gets or sets the HttpContext for the current request.
    /// </summary>
    public HttpContext Context { get; set; } = default!;

    /// <summary>
    /// Gets the current user from the HttpContext.
    /// </summary>
    public ClaimsPrincipal User => Context?.User ?? new ClaimsPrincipal();

    /// <summary>
    /// Returns an OK (200) result.
    /// </summary>
    protected IResult Ok() => Results.Ok();

    /// <summary>
    /// Returns an OK (200) result with value.
    /// </summary>
    protected IResult Ok<T>(T value) => Results.Ok(value);

    /// <summary>
    /// Returns a Created (201) result.
    /// </summary>
    protected IResult Created(string uri, object? value) => Results.Created(uri, value);

    /// <summary>
    /// Returns a Created (201) result.
    /// </summary>
    protected IResult Created<T>(string uri, T value) => Results.Created(uri, value);

    /// <summary>
    /// Returns a NotFound (404) result.
    /// </summary>
    protected IResult NotFound() => Results.NotFound();

    /// <summary>
    /// Returns a NotFound (404) result with value.
    /// </summary>
    protected IResult NotFound<T>(T value) => Results.NotFound(value);

    /// <summary>
    /// Returns a BadRequest (400) result.
    /// </summary>
    protected IResult BadRequest() => Results.BadRequest();

    /// <summary>
    /// Returns a BadRequest (400) result with error.
    /// </summary>
    protected IResult BadRequest<T>(T error) => Results.BadRequest(error);

    /// <summary>
    /// Returns a Unauthorized (401) result.
    /// </summary>
    protected IResult Unauthorized() => Results.Unauthorized();

    /// <summary>
    /// Returns a Forbidden (403) result.
    /// </summary>
    protected IResult Forbid() => Results.Forbid();

    /// <summary>
    /// Returns a NoContent (204) result.
    /// </summary>
    protected IResult NoContent() => Results.NoContent();

    /// <summary>
    /// Returns a Problem (500) result.
    /// </summary>
    protected IResult Problem(string? detail = null, string? instance = null, int? statusCode = null, string? title = null, string? type = null, System.Collections.Generic.IDictionary<string, object?>? extensions = null)
        => Results.Problem(detail, instance, statusCode, title, type, extensions);
}
