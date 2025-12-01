using Microsoft.AspNetCore.Mvc;
using MinimalEndpoints.Attributes;
using MinimalEndpoints.Example.Models;
using MinimalEndpoints.Example.Services;

namespace MinimalEndpoints.Example.Endpoints;

[MinimalEndpoints("api/users")]
[EndpointGroupName("User Management")]
public class UpdateUserEndpoint : BaseMinimalApiEndpoint
{
    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserRequest request, [FromServices] IUserService userService)
    {
        await Task.Delay(1);
        var user = userService.Update(id, request.Name);
        return user != null ? Results.Ok(user) : Results.NotFound();
    }
}
