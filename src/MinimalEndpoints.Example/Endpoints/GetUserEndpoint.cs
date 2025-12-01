using Microsoft.AspNetCore.Mvc;
using MinimalEndpoints.Attributes;
using MinimalEndpoints.Example.Models;
using MinimalEndpoints.Example.Services;

namespace MinimalEndpoints.Example.Endpoints;

[MinimalEndpoints("api/users")]
[EndpointGroupName("User Management")]
public class GetUserEndpoint : BaseMinimalApiEndpoint
{
    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="userService">The user service</param>
    /// <returns>The user if found, or null</returns>
    /// <response code="200">User found or null if not found</response>
    [HttpGet("{id}")]
    [Produces("application/json")]
    public async Task<User?> GetUser([FromRoute] int id, [FromServices] IUserService userService)
    {
        await Task.Delay(1);
        return userService.Get(id);
    }
}
