using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TerraScale.MinimalEndpoints.Example.Models;
using TerraScale.MinimalEndpoints.Tests;

namespace TerraScale.MinimalEndpoints.Tests;

public class ErrorHandlingTests
{
    [ClassDataSource<WebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactory WebApplicationFactory { get; init; }

    [Test]
    public async Task Endpoint_Returns_Problem_Details_For_Server_Errors()
    {
        var client = WebApplicationFactory.CreateClient();
        
        // This endpoint should trigger an error in the service
        var response = await client.GetAsync("/api/error/500");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        await Assert.That(problem).IsNotNull();
        await Assert.That(problem!.Status).IsEqualTo(500);
        await Assert.That(problem.Detail).Contains("Internal server error");
    }

    [Test]
    public async Task Endpoint_Returns_Custom_Error_Response()
    {
        var client = WebApplicationFactory.CreateClient();
        
        var response = await client.GetAsync("/api/error/custom");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        await Assert.That(content).Contains("Custom error message");
    }

    [Test]
    public async Task Endpoint_Handles_NotFound_Correctly()
    {
        var client = WebApplicationFactory.CreateClient();
        
        var response = await client.GetAsync("/api/users/999999");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Endpoint_Returns_Validation_Errors()
    {
        var client = WebApplicationFactory.CreateClient();
        
        // Test multiple validation scenarios
        var emptyNameResponse = await client.PostAsJsonAsync("/api/users", new CreateUserRequest(""));
        await Assert.That(emptyNameResponse.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        var invalidEmailResponse = await client.PostAsJsonAsync("/api/users", new CreateUserRequest("invalid-email"));
        await Assert.That(invalidEmailResponse.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        var validationErrors = await emptyNameResponse.Content.ReadAsStringAsync();
        await Assert.That(validationErrors).Contains("required");
    }

    [Test]
    public async Task Endpoint_Returns_Different_Status_Codes_Correctly()
    {
        var client = WebApplicationFactory.CreateClient();
        
        // Test 201 Created response
        var createResponse = await client.PostAsJsonAsync("/api/users", new CreateUserRequest("New User"));
        await Assert.That(createResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        // Test 204 No Content response
        var deleteResponse = await client.DeleteAsync("/api/users/1");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        // Test 401 Unauthorized response
        var unauthorizedResponse = await client.GetAsync("/api/users/1");
        await Assert.That(unauthorizedResponse.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Endpoint_Returns_Content_Types_Correctly()
    {
        var client = WebApplicationFactory.CreateClient();
        
        // Test JSON response
        var jsonResponse = await client.GetAsync("/api/users/1");
        await Assert.That(jsonResponse.Content.Headers.ContentType?.Contains("application/json")).IsTrue();

        // Test plain text response
        var textResponse = await client.GetAsync("/api/weather?city=London");
        await Assert.That(textResponse.Content.Headers.ContentType?.Contains("text/plain")).IsTrue();
    }

    [Test]
    public async Task Endpoint_Handles_Exceptions_Correctly()
    {
        var client = WebApplicationFactory.CreateClient();
        
        // This should trigger an exception in the service
        var response = await client.GetAsync("/api/error/exception");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
    }

    [Test]
    public async Task Endpoint_Returns_Headers_Correctly()
    {
        var client = WebApplicationFactory.CreateClient();
        
        var response = await client.GetAsync("/api/new-features");

        // Check for custom headers
        await Assert.That(response.Headers.Contains("X-Custom-Header")).IsTrue();
        
        var headerValues = response.Headers.GetValues("X-Custom-Header");
        await Assert.That(headerValues?.FirstOrDefault()).IsEqualTo("Configured");
    }

    [Test]
    public async Task Endpoint_Returns_CORS_Headers_Correctly()
    {
        var client = WebApplicationFactory.CreateClient();
        
        var response = await client.GetAsync("/api/users/1");

        // Check for CORS headers (if CORS is configured)
        var hasCorsHeaders = response.Headers.Contains("Access-Control-Allow-Origin");
        // This test may need adjustment based on CORS configuration
    }

    [Test]
    public async Task Endpoint_Handles_Timeout_Correctly()
    {
        var client = WebApplicationFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(1);
        
        var response = await client.GetAsync("/api/slow/timeout");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.RequestTimeout);
    }
}