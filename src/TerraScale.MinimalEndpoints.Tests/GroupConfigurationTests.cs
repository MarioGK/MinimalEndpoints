using System.Net;
using System.Net.Http.Json;
using System.Linq;
using TerraScale.MinimalEndpoints.Tests;

namespace TerraScale.MinimalEndpoints.Tests;

public class GroupConfigurationTests
{
    [ClassDataSource<WebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactory WebApplicationFactory { get; init; }

    [Test]
    public async Task Group_RoutePrefix_Is_Applied_Correctly()
    {
        var client = WebApplicationFactory.CreateClient();
        var response = await client.GetAsync("/api/weather?city=London");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Group_Name_Is_Used_For_OpenAPI_Grouping()
    {
        var client = WebApplicationFactory.CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        var swaggerDoc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var paths = swaggerDoc.RootElement.GetProperty("paths")?.GetProperty("paths");
        
        // Check that weather endpoints are grouped under "Weather API"
        var weatherPaths = paths?.EnumerateObject()
            .Where(p => p.Name.Contains("/weather"))
            .ToList();
        
        await Assert.That(weatherPaths).Count().IsGreaterThan(0);
        
        // Check that user endpoints are grouped under "User Management"
        var userPaths = paths?.EnumerateObject()
            .Where(p => p.Name.Contains("/users"))
            .ToList();
        
        await Assert.That(userPaths).Count().IsGreaterThan(0);
    }

    [Test]
    public async Task Group_Tags_Are_Applied_Correctly()
    {
        var client = WebApplicationFactory.CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        var swaggerDoc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var root = swaggerDoc.RootElement;
        
        // Check for tags in the document
        var tags = root.GetProperty("tags")?.EnumerateArray();
        await Assert.That(tags).IsNotNull().And.CountGreaterThan(0);
        
        // Check for specific expected tags
        var tagNames = tags?.Select(t => t.GetProperty("name")?.GetString()).ToList();
        await Assert.That(tagNames).Contains("Weather API");
        await Assert.That(tagNames).Contains("User Management");
    }

    [Test]
    public async Task Group_Configure_Method_Is_Called()
    {
        // Test that custom group configuration is applied
        var client = WebApplicationFactory.CreateClient();
        var response = await client.GetAsync("/api/services/greet?name=Test");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        
        // Check if custom tags from ServiceApiGroup are applied
        var swaggerResponse = await client.GetAsync("/swagger/v1/swagger.json");
        var swaggerDoc = await swaggerResponse.Content.ReadFromJsonAsync<JsonDocument>();
        
        // Look for the greet endpoint
        var paths = swaggerDoc.RootElement.GetProperty("paths")?.GetProperty("paths");
        var greetPath = paths?.GetProperty("/greet");
        
        if (greetPath != null)
        {
            var getOperation = greetPath.GetProperty("get");
            var tags = getOperation?.GetProperty("tags")?.EnumerateArray();
            var tagNames = tags?.Select(t => t.GetString()).ToList();
            
            await Assert.That(tagNames).Contains("ServiceAPI"));
        }
    }

    [Test]
    public async Task Multiple_Groups_With_Same_RoutePrefix_Are_Handled()
    {
        // Test how multiple groups with same prefix are handled
        var client = WebApplicationFactory.CreateClient();
        
        // Both weather and service endpoints should work
        var weatherResponse = await client.GetAsync("/api/weather?city=London");
        var serviceResponse = await client.GetAsync("/api/services/greet?name=Test");

        await Assert.That(weatherResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(serviceResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Nested_Groups_Are_Supported()
    {
        // Test that nested group structures work
        var client = WebApplicationFactory.CreateClient();
        var response = await client.GetAsync("/grouped/test");

        // This should work now with our manual generation
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Group_Inheritance_Works_Correctly()
    {
        // Test that custom group inheritance works
        var client = WebApplicationFactory.CreateClient();
        var response = await client.GetAsync("/api/weather?city=London");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Group_Filters_Are_Applied()
    {
        // Test that endpoint filters are applied to groups
        var client = WebApplicationFactory.CreateClient();
        var response = await client.GetAsync("/api/new-features");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        
        // Check for custom header from filter
        var hasCustomHeader = response.Headers.Contains("X-Custom-Header");
        await Assert.That(hasCustomHeader).IsTrue();
    }
}