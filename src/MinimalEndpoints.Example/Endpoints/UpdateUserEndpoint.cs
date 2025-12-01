using Microsoft.AspNetCore.Mvc;
using MinimalEndpoints.Example.Models;
using MinimalEndpoints.Example.Services;
using TerraScale.MinimalEndpoints.Attributes;

namespace MinimalEndpoints.Example.Endpoints;

[MinimalEndpoints("api/users")]
[Attributes.EndpointGroupName("User Management")]
public class UpdateUserEndpoint : BaseMinimalApiEndpoint
{
    [HttpPut("{id}")]
    [Attributes.Produces("application/json", StatusCode = 200)]
    [Attributes.Consumes("application/json")]
    public async Task<IResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserRequest request, [FromServices] IUserService userService)
    {
        await Task.Delay(1);
        var user = userService.Update(id, request.Name);
        return user != null ? Results.Ok(user) : Results.NotFound();
    }
}
