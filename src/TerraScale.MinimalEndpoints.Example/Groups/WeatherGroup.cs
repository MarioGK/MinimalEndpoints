using TerraScale.MinimalEndpoints;
using TerraScale.MinimalEndpoints.Groups;

namespace TerraScale.MinimalEndpoints.Example.Groups;

public class WeatherGroup : EndpointGroup
{
    public override string Name => "Weather API";
    public override string RoutePrefix => "/api/weather";
    public override void Configure(RouteGroupBuilder builder)
    {
        
    }
}
