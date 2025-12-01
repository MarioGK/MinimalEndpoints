using Microsoft.AspNetCore.Mvc;
using MinimalEndpoints.Attributes;
using MinimalEndpoints.Example.Services;

namespace MinimalEndpoints.Example.Endpoints;

[MinimalEndpoints("api/users")]
[EndpointGroupName("User Management")]
public class DeleteUserEndpoint : BaseMinimalApiEndpoint
{
    [HttpDelete("{id}")]
    [Produces("application/json")]
    public async Task<IResult> DeleteUser([FromRoute] int id, [FromServices] IUserService userService)
    {
        await Task.Delay(1);
        var result = userService.Delete(id);
        return Results.Ok(result); // Returns boolean true/false
    }
}
