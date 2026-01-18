using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dilcore.WebApi.IntegrationTests.Infrastructure;
using Shouldly;

namespace Dilcore.WebApi.IntegrationTests;

/// <summary>
/// Integration tests for the health check endpoint.
/// </summary>
[TestFixture]
public class HealthCheckTests : BaseIntegrationTest
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
    public async Task HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task HealthCheck_ReturnsCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }

    [Test]
    public async Task HealthCheck_ReturnsHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        content.TryGetProperty("status", out var status).ShouldBeTrue();
        status.GetString().ShouldBe("Healthy");
    }

    [Test]
    public async Task HealthCheck_ReturnsTimestamp()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        content.TryGetProperty("timestamp", out var timestamp).ShouldBeTrue();
        timestamp.GetDateTime().ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(-1));
        timestamp.GetDateTime().ShouldBeLessThan(DateTime.UtcNow.AddMinutes(1));
    }

    [Test]
    public async Task HealthCheck_ReturnsServiceName()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        content.TryGetProperty("service", out var service).ShouldBeTrue();
        service.GetString().ShouldBe("Dilcore Platform");
    }

    [Test]
    public async Task HealthCheck_DoesNotRequireAuthentication()
    {
        // Arrange - create client without authentication
        var anonymousClient = Factory.CreateClient();

        // Act
        var response = await anonymousClient.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task HealthCheck_DoesNotRequireTenantHeader()
    {
        // Arrange - client without tenant header
        var clientWithoutTenant = Factory.CreateClient();

        // Act
        var response = await clientWithoutTenant.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task HealthCheck_ResponseHasCorrectStructure()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert - verify all expected properties exist
        content.TryGetProperty("status", out _).ShouldBeTrue();
        content.TryGetProperty("timestamp", out _).ShouldBeTrue();
        content.TryGetProperty("service", out _).ShouldBeTrue();
    }
}
