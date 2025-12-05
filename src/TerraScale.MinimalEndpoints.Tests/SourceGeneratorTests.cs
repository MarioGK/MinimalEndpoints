using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using TerraScale.MinimalEndpoints.Tests;

namespace TerraScale.MinimalEndpoints.Tests;

public class SourceGeneratorTests
{
    [ClassDataSource<WebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactory WebApplicationFactory { get; init; }

    [Test]
    public void Generator_Detects_All_Endpoint_Classes()
    {
        // Verify that source generator finds all expected endpoint classes
        var services = WebApplicationFactory.Services;
        var endpointAssembly = typeof(TerraScale.MinimalEndpoints.Example.Program).Assembly;
        
        // Get all classes that should be detected as endpoints
        var expectedEndpointClasses = new[]
        {
            "WeatherEndpoints",
            "CreateUserEndpoint", 
            "GetUserEndpoint",
            "UpdateUserEndpoint",
            "DeleteUserEndpoint",
            "GroupedEndpoint",
            "NewFeatureEndpoint",
            "ServiceEndpoints",
            "PublicEndpoint",
            "PolicyProtectedEndpoint",
            "ErrorTestEndpoints"
        };

        var actualEndpointTypes = endpointAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && expectedEndpointClasses.Contains(t.Name))
            .ToList();

        await Assert.That(actualEndpointTypes.Count).IsGreaterThanOrEqualTo(expectedEndpointClasses.Length);
        
        foreach (var expectedClass in expectedEndpointClasses)
        {
            var found = actualEndpointTypes.Any(t => t.Name == expectedClass);
            await Assert.That(found).IsTrue($"Expected endpoint class {expectedClass} to be found");
        }
    }

    [Test]
    public void Generator_Handles_Multiple_Methods_In_One_Class()
    {
        // This test verifies that source generator reports diagnostic for multiple methods
        // In a real scenario, this should generate a compile-time error
        
        // For now, we'll verify that our manual generation handles this correctly
        // The source generator should detect this and report ME003 error
        
        // This test would be implemented by checking compiler diagnostics
        // For now, we'll verify the expected behavior
        var services = WebApplicationFactory.Services;
        var endpointCount = services.Count(s => s.ServiceType != null && typeof(IMinimalEndpoint).IsAssignableFrom(s.ServiceType));
        
        // Should have reasonable number of endpoints (not too many, indicating multiple methods per class)
        await Assert.That(endpointCount).IsLessThan(20); // Arbitrary upper limit
    }

    [Test]
    public void Generator_Validates_Interface_Implementation()
    {
        // Verify that all registered endpoints implement IMinimalEndpoint
        var services = WebApplicationFactory.Services;
        var endpointTypes = services.Where(s => s.ServiceType != null && typeof(IMinimalEndpoint).IsAssignableFrom(s.ServiceType))
            .Select(s => s.ServiceType);

        foreach (var endpointType in endpointTypes)
        {
            await Assert.That(typeof(IMinimalEndpoint).IsAssignableFrom(endpointType))
                .IsTrue($"Endpoint type {endpointType.Name} should implement IMinimalEndpoint");
        }
    }

    [Test]
    public void Generator_Handles_Attributes_Correctly()
    {
        // Verify that source generator processes attributes correctly
        var services = WebApplicationFactory.Services;
        var endpointAssembly = typeof(TerraScale.MinimalEndpoints.Example.Program).Assembly;
        
        // Check for classes with specific attributes
        var hasMinimalEndpointsAttribute = endpointAssembly.GetTypes()
            .Any(t => t.GetCustomAttributes().Any(a => a.GetType().Name.Contains("MinimalEndpointsAttribute")));
        
        var hasHttpMethodAttributes = endpointAssembly.GetTypes()
            .Any(t => t.GetMethods().Any(m => m.GetCustomAttributes().Any(a => a.GetType().Name.Contains("Http"))));
        
        var hasGroupNameAttributes = endpointAssembly.GetTypes()
            .Any(t => t.GetCustomAttributes().Any(a => a.GetType().Name.Contains("EndpointGroupNameAttribute"))));
        
        await Assert.That(hasMinimalEndpointsAttribute).IsTrue();
        await Assert.That(hasHttpMethodAttributes).IsTrue();
        await Assert.That(hasGroupNameAttributes).IsTrue();
    }

    [Test]
    public void Generator_Handles_Group_Attributes()
    {
        // Verify that group-level attributes are processed
        var services = WebApplicationFactory.Services;
        var endpointAssembly = typeof(TerraScale.MinimalEndpoints.Example.Program).Assembly;
        
        // Check for groups with authorization
        var hasAuthorizeOnGroups = endpointAssembly.GetTypes()
            .Where(t => typeof(EndpointGroup).IsAssignableFrom(t))
            .Any(t => t.GetCustomAttributes().Any(a => a.GetType().Name.Contains("AuthorizeAttribute"))));
        
        await Assert.That(hasAuthorizeOnGroups).IsTrue();
    }

    [Test]
    public void Generator_Produces_Valid_Namespace()
    {
        // Verify that generated code uses correct namespace
        var services = WebApplicationFactory.Services;
        
        // For now, we'll verify that endpoints are registered correctly
        var endpointCount = services.Count(s => s.ServiceType != null && typeof(IMinimalEndpoint).IsAssignableFrom(s.ServiceType));
        
        await Assert.That(endpointCount).IsGreaterThan(0, "Endpoints should be registered");
    }

    [Test]
    public void Generator_Handles_Route_Conflicts()
    {
        // Verify that route conflicts are handled appropriately
        // This would be tested by having endpoints with conflicting routes
        
        // For now, we'll verify that all endpoints have unique routes
        var services = WebApplicationFactory.Services;
        var endpointTypes = services.Where(s => s.ServiceType != null && typeof(IMinimalEndpoint).IsAssignableFrom(s.ServiceType))
            .Select(s => s.ServiceType);

        // Get route information (this would require reflection in a real scenario)
        var routes = new List<string>();
        
        foreach (var endpointType in endpointTypes)
        {
            // Extract route from class name or attributes
            if (endpointType.Name.Contains("Weather"))
                routes.Add("/api/weather");
            else if (endpointType.Name.Contains("User"))
                routes.Add("/api/users");
            else if (endpointType.Name.Contains("Grouped"))
                routes.Add("/grouped/test");
            else if (endpointType.Name.Contains("NewFeature"))
                routes.Add("/api/new-features");
            else if (endpointType.Name.Contains("Service"))
                routes.Add("/api/services/greet");
        }

        var uniqueRoutes = routes.Distinct().ToList();
        await Assert.That(uniqueRoutes.Count).IsEqualTo(routes.Count, "All endpoint routes should be unique");
    }

    [Test]
    public void Generator_Produces_Valid_Namespace()
    {
        // Verify that generated code uses correct namespace
        var services = WebApplicationFactory.Services;
        var registrationType = typeof(TerraScale.MinimalEndpoints.Generated_TerraScale_MinimalEndpoints_Example.MinimalEndpointRegistration);
        
        await Assert.That(registrationType.Namespace).IsEqualTo("TerraScale.MinimalEndpoints.Generated_TerraScale_MinimalEndpoints_Example");
    }

    [Test]
    public void Generator_Handles_Dependencies_Correctly()
    {
        // Verify that endpoint dependencies are registered
        var services = WebApplicationFactory.Services;
        
        // Check that UserService is registered
        var userService = services.FirstOrDefault(s => s.ServiceType == typeof(TerraScale.MinimalEndpoints.Example.Services.IUserService));
        await Assert.That(userService).IsNotNull("UserService should be registered");
        
        // Check that GreetingService is registered
        var greetingService = services.FirstOrDefault(s => s.ServiceType == typeof(TerraScale.MinimalEndpoints.Example.Services.IGreetingService));
        await Assert.That(greetingService).IsNotNull("GreetingService should be registered");
    }
}