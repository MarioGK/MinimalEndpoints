using Microsoft.AspNetCore.Mvc;
using TerraScale.MinimalEndpoints.Example.Models;
using TerraScale.MinimalEndpoints.Example.Services;

namespace TerraScale.MinimalEndpoints.Example.Endpoints;

public class Error500Endpoint : BaseMinimalApiEndpoint
{
    public override string Route => "api/error/500";
    public override EndpointHttpMethod HttpMethod => EndpointHttpMethod.Get;

    public Task<ProblemDetails> InternalServerError()
    {
        return Task.FromResult(new ProblemDetails
        {
            Status = 500,
            Title = "Internal Server Error",
            Detail = "Internal server error occurred"
        });
    }
}

public class ErrorCustomEndpoint : BaseMinimalApiEndpoint
{
    public override string Route => "api/error/custom";
    public override EndpointHttpMethod HttpMethod => EndpointHttpMethod.Get;

    public Task<string> CustomError()
    {
        return Task.FromResult("Custom error message");
    }
}

public class ErrorExceptionEndpoint : BaseMinimalApiEndpoint
{
    public override string Route => "api/error/exception";
    public override EndpointHttpMethod HttpMethod => EndpointHttpMethod.Get;

    public Task<string> ThrowException()
    {
        throw new InvalidOperationException("Test exception");
    }
}

public class ErrorTimeoutEndpoint : BaseMinimalApiEndpoint
{
    public override string Route => "api/error/timeout";
    public override EndpointHttpMethod HttpMethod => EndpointHttpMethod.Get;

    public Task<string> SlowEndpoint()
    {
        // Simulate a slow operation
        Task.Delay(TimeSpan.FromSeconds(2)).Wait();
        return Task.FromResult("Response after delay");
    }
}