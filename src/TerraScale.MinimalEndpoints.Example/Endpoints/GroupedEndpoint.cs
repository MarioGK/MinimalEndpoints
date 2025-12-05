using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TerraScale.MinimalEndpoints;
using TerraScale.MinimalEndpoints.Groups;
using TerraScale.MinimalEndpoints.Example.Groups;

namespace TerraScale.MinimalEndpoints.Example.Endpoints;

public class GroupedEndpoint : BaseMinimalApiEndpoint<MyTestGroup>
{
    public override string Route => "test";
    public override EndpointHttpMethod HttpMethod => EndpointHttpMethod.Get;

    public Task<string> Handle()
    {
        return Task.FromResult("Grouped!");
    }
}
