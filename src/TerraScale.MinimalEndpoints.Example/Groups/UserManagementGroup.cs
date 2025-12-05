using TerraScale.MinimalEndpoints;
using TerraScale.MinimalEndpoints.Groups;

namespace TerraScale.MinimalEndpoints.Example.Groups;

public class UserManagementGroup : EndpointGroup
{
    public override string Name => "User Management";
    public override string RoutePrefix => "/api/users";
    public override void Configure(RouteGroupBuilder builder)
    {
        
    }
}
