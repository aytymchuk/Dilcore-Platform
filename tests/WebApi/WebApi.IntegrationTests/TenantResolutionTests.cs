using System.Net;
using Dilcore.Results.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.WebApi.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dilcore.WebApi.IntegrationTests;

[TestFixture]
public class TenantResolutionTests : BaseIntegrationTest
{
    private const string TestTenantId = "t1";
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task SetUpClient()
    {
        _client = Factory.CreateClient();

        // Create test tenant via Orleans grain
        using var scope = Factory.Services.CreateScope();
        var grainFactory = scope.ServiceProvider.GetRequiredService<IGrainFactory>();
        var tenantGrain = grainFactory.GetGrain<ITenantGrain>(TestTenantId);
        await tenantGrain.CreateAsync("Test Tenant", "Tenant for integration tests");
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

        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = System.Text.Json.JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content);

        problemDetails.ShouldNotBeNull();
        problemDetails.Title.ShouldBe("Tenant Not Resolved");
        problemDetails.Status.ShouldBe(400);
        problemDetails.Extensions.ShouldNotBeNull();
        var errorCode = problemDetails.Extensions["errorCode"]?.ToString();
        errorCode.ShouldBe(ProblemDetailsConstants.TenantNotResolved);
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
        // Ensure endpoints not explicitly excluded require tenant header and return BadRequest when missing
        var response = await _client.GetAsync("/weatherforecast");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
