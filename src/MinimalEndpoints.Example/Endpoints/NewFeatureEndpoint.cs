using Microsoft.AspNetCore.Mvc;
using MinimalEndpoints;
using MinimalEndpoints.Attributes;

namespace MinimalEndpoints.Example.Endpoints;

[MinimalEndpoints("api/new-features")]
public class NewFeatureEndpoint : BaseMinimalApiEndpoint
{
    [HttpGet("/api/new-features")]
    public async Task<IResult> Handle()
    {
        await Task.CompletedTask;

        // Check if HttpContext is injected
        if (Context == null)
            return Results.Problem("Context is null");

        // Check helper methods
        return Ok(new { Message = "Success", User = User.Identity?.Name ?? "Anonymous" });
    }

    public static void Configure(RouteHandlerBuilder builder)
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            context.HttpContext.Response.Headers.Append("X-Custom-Header", "Configured");
            return await next(context);
        });
    }
}
