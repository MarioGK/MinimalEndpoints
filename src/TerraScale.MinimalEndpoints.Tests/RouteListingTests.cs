using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TerraScale.MinimalEndpoints.Tests;

namespace TerraScale.MinimalEndpoints.Tests;

public class RouteListingTests
{
    [ClassDataSource<WebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactory WebApplicationFactory { get; init; }

    [Test]
    public async Task ListAllRoutes()
    {
        var client = WebApplicationFactory.CreateClient();
        
        // Try to access the grouped endpoint
        var response = await client.GetAsync("/grouped/test");
        
        // Let's see what we actually get
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Content: {content}");
        
        // Also try some other endpoints to see what works
        var weatherResponse = await client.GetAsync("/api/weather?city=London");
        Console.WriteLine($"Weather Status: {weatherResponse.StatusCode}");
        
        await Assert.That(weatherResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }
}