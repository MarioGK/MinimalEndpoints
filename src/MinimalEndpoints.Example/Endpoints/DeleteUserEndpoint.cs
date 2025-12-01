using Microsoft.AspNetCore.Mvc;
using MinimalEndpoints.Example.Services;
using TerraScale.MinimalEndpoints.Attributes;

namespace MinimalEndpoints.Example.Endpoints;

[MinimalEndpoints("api/users")]
[Attributes.EndpointGroupName("User Management")]
public class DeleteUserEndpoint : BaseMinimalApiEndpoint
{
    [HttpDelete("{id}")]
    [Attributes.Produces("application/json", StatusCode = 200)]
    public async Task<IResult> DeleteUser([FromRoute] int id, [FromServices] IUserService userService)
    {
        await Task.Delay(1);
        var result = userService.Delete(id);
        return Results.Ok(result); // Returns boolean true/false
    }
}
