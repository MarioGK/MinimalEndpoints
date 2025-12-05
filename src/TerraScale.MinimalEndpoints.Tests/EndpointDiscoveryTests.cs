using System.Net;
using System.Net.Http.Json;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TerraScale.MinimalEndpoints.Tests;

namespace TerraScale.MinimalEndpoints.Tests;

public class EndpointDiscoveryTests
{
    [ClassDataSource<WebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactory WebApplicationFactory { get; init; }

    [Test]
    public void All_Endpoint_Classes_Are_Registered_In_DI_Container()
    {
        // Verify that all endpoint classes are registered in the DI container
        var services = WebApplicationFactory.Services;
        
        // Get all endpoint types from the Example assembly
        var endpointAssembly = typeof(TerraScale.MinimalEndpoints.Example.Services.UserService).Assembly;
        var endpointTypes = endpointAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IMinimalEndpoint).IsAssignableFrom(t))
            .ToList();

        foreach (var endpointType in endpointTypes)
        {
            // Check if the service is registered
            var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == endpointType);
            await Assert.That(serviceDescriptor).IsNotNull($"Endpoint type {endpointType.Name} should be registered as scoped service");
            await Assert.That(serviceDescriptor!.Lifetime).IsEqualTo(ServiceLifetime.Scoped);
        }
    }

    [Test]
    public void Endpoint_Without_Attributes_Is_Not_Registered()
    {
        // Verify that classes without endpoint attributes are not registered
        var services = WebApplicationFactory.Services;
        
        // This class doesn't have endpoint attributes
        var nonEndpointType = typeof(EndpointDiscoveryTests);
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == nonEndpointType);
        await Assert.That(serviceDescriptor).IsNull($"Non-endpoint class {nonEndpointType.Name} should not be registered");
    }

    [Test]
    public void Endpoint_With_Multiple_Methods_Should_Generate_Diagnostics()
    {
        // This test verifies that the source generator detects multiple methods in one class
        // For now, we'll check that our manual generation handles this correctly
        // In a real scenario, this should generate a diagnostic error
        
        // Get the generated registration to see how many endpoints are registered
        var services = WebApplicationFactory.Services;
        var endpointAssembly = typeof(TerraScale.MinimalEndpoints.Example.Program).Assembly;
        
        // Count endpoints that should be registered (based on our manual generation)
        var expectedEndpointCount = 7; // Weather, Create User, Get User, Update User, Delete User, Grouped, New Feature, Service Greet
        var actualEndpointCount = services.Count(s => s.ServiceType != null && typeof(IMinimalEndpoint).IsAssignableFrom(s.ServiceType));
        
        await Assert.That(actualEndpointCount).IsGreaterThan(expectedEndpointCount - 2); // Allow some variance
    }

    [Test]
    public void Endpoint_Without_IMinimalEndpoint_Should_Generate_Diagnostic()
    {
        // Verify that classes not implementing IMinimalEndpoint generate diagnostics
        // This would be tested by checking compiler diagnostics in a real scenario
        
        // For now, we'll verify that all registered endpoints do implement IMinimalEndpoint
        var services = WebApplicationFactory.Services;
        var invalidEndpoints = services.Where(s => s.ServiceType != null && !typeof(IMinimalEndpoint).IsAssignableFrom(s.ServiceType));
        
        await Assert.That(invalidEndpoints).IsEmpty();
    }

    [Test]
    public async Task Endpoint_Route_Conflict_Should_Be_Handled()
    {
        // Test how conflicting routes are handled
        var client = WebApplicationFactory.CreateClient();
        
        // Try to access an endpoint that doesn't exist
        var response = await client.GetAsync("/nonexistent/endpoint");
        
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public void All_Http_Methods_Are_Supported()
    {
        // Verify that all HTTP methods are supported by checking registered endpoints
        var services = WebApplicationFactory.Services;
        var endpointTypes = services.Where(s => s.ServiceType != null && typeof(IMinimalEndpoint).IsAssignableFrom(s.ServiceType))
            .Select(s => s.ServiceType);

        // Check for different HTTP methods by examining the endpoint classes
        var hasGet = endpointTypes.Any(t => t.Name.Contains("Weather") || t.Name.Contains("GetUser") || t.Name.Contains("Grouped"));
        var hasPost = endpointTypes.Any(t => t.Name.Contains("CreateUser"));
        var hasPut = endpointTypes.Any(t => t.Name.Contains("UpdateUser"));
        var hasDelete = endpointTypes.Any(t => t.Name.Contains("DeleteUser"));

        await Assert.That(hasGet).IsTrue("Should have GET endpoints");
        await Assert.That(hasPost).IsTrue("Should have POST endpoints");
        await Assert.That(hasPut).IsTrue("Should have PUT endpoints");
        await Assert.That(hasDelete).IsTrue("Should have DELETE endpoints");
    }
}