using System.Net;
using System.Net.Http.Json;
using Dilcore.WebApi.IntegrationTests.Infrastructure;
using Shouldly;

namespace Dilcore.WebApi.IntegrationTests;

[TestFixture]
public class TenantResolutionTests : BaseIntegrationTest
{
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void SetUpClient()
    {
        _client = Factory.CreateClient();
    }

    [OneTimeTearDown]
    public void TearDownClient()
    {
        _client.Dispose();
    }

    [Test]
    public async Task GetProtectedEndpoint_WithoutTenantHeader_ReturnsBadRequest()
    {
        // Act
        // /test/tenant-info endpoint attempts to resolve tenant and manual check.
        // But with our change, RESOLUTION itself throws.
        var response = await _client.GetAsync("/test/tenant-info");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
        problemDetails.ShouldNotBeNull();
        problemDetails.Title.ShouldBe("Tenant Not Resolved");
        problemDetails.Status.ShouldBe(400);

        // Use JsonElement to access extensions dynamically avoiding complexity with dictionary serialization
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain(Constants.ProblemDetails.TenantNotResolved);
    }

    [Test]
    public async Task GetProtectedEndpoint_WithTenantHeader_ReturnsOk()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/test/tenant-info");
        request.Headers.Add("x-tenant", "t1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetPublicEndpoint_WithoutTenantHeader_ReturnsOk()
    {
        // Act
        // Public endpoint excludes itself from multi-tenant resolution?
        // Wait, if it *excludes* itself, it shouldn't trigger Resolve().
        // /test/public-info explicitly calls .ExcludeFromMultiTenantResolution()
        // If it doesn't try to access DI ITenantContext, it won't throw.
        var response = await _client.GetAsync("/test/public-info");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetWeatherForecast_WithoutTenantHeader_ReturnsBadRequest()
    {
        // Act
        // This endpoint is NOT excluded, and likely resolving tenant via middleware/pipeline implicitly?
        // Wait, does it use ITenantContext? No.
        // Does anything else trigger resolution?
        // If nothing triggers Resolve(), it won't fail!
        // The user asked "if tenant header is not passed when the endpoint not excluded - should be returned error".
        // But if the endpoint code doesn't USE tenant info, Resolve() is currently lazily called by DI injection.
        // If WeatherForecast doesn't inject it, it WON'T throw.

        // NOTE: This test might FAIL expectation if no middleware forces resolution.
        // But let's check. If it succeeds (returns OK), then we aren't strict enough yet?
        // Or maybe something else (OTEL processor?) is triggering it?
        // But we added try-catch to OTEL!

        // So unless we have middleware that forces resolution, this might pass.
        // But let's verify assumption.

        var response = await _client.GetAsync("/weatherforecast");

        // If assumption is strict enforcement, this SHOULD be Bad Request.
        // If it returns OK, I'll need to explain or adjust.
        // For now let's assert what logical strictness implies.

        // Actually, if it returns OK, then we haven't achieved "Mandatory Tenant if not excluded".
        // Use case: Developer forgets to exclude, but also forgets to use tenant. Safe?
        // Or developer forgets to exclude, but endpoint shouldn't be accessible without tenant context established?

        // Let's assert BadRequest and see if it fails.
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
