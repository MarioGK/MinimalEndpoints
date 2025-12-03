using System.Net;
using TerraScale.MinimalEndpoints.Tests;

namespace TerraScale.MinimalEndpoints.Tests;

public class GroupTests
{
    [ClassDataSource<WebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactory WebApplicationFactory { get; init; }

    [Test]
    public async Task GroupedEndpoint_IsProtected()
    {
        var client = WebApplicationFactory.CreateClient();
        // The route is /grouped/test
        var response = await client.GetAsync("/grouped/test");

        // Should be 401 Unauthorized because we didn't provide credentials
        // and the group has [Authorize(Roles="Admin")]
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }
}
