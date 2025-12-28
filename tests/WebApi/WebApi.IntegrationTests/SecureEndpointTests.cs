using System.Net;
using Shouldly;

namespace WebApi.IntegrationTests;

[TestFixture]
public class SecureEndpointTests
{
    private CustomWebApplicationFactory _factory;
    private HttpClient _client;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetWeatherForecast_WhenAuthenticated_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public async Task GetWeatherForecast_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Unauthorized", "true");

        // Act
        var response = await client.GetAsync("/weatherforecast");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
