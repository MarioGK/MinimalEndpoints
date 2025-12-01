using Microsoft.AspNetCore.Mvc;
using TerraScale.MinimalEndpoints.Attributes;

namespace MinimalEndpoints.Example.Endpoints;

[MinimalEndpoints("api/weather")]
[Attributes.EndpointGroupName("Weather API")]
public class WeatherEndpoints : BaseMinimalApiEndpoint
{
    /// <summary>
    /// Gets weather information for a city
    /// </summary>
    /// <param name="city">The city name to get weather for</param>
    /// <returns>Weather information for the specified city</returns>
    /// <remarks>
    /// This endpoint provides a simple weather forecast.
    /// Currently returns a static sunny weather for all cities.
    /// </remarks>
    /// <response code="200">Weather information retrieved successfully</response>
    /// <response code="400">Invalid city name provided</response>
    [HttpGet]
    [Attributes.Produces("application/json", "text/plain", StatusCode = 200)]
    public async Task<string> GetWeather([FromQuery] string city)
    {
        await Task.Delay(1); // Simulate async work
        return $"Weather in {city} is Sunny";
    }
}
