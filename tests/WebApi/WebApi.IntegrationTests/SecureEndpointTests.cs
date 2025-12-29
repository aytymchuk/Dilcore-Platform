using System.Net;
using Shouldly;

namespace Dilcore.WebApi.IntegrationTests;

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
        factory.FakeUser.IsAuthenticated = false;
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/weatherforecast");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetWeatherForecast_WithCustomUser_ReturnsOk()
    {
        // Arrange - Configure factory with custom user
        using var factory = new CustomWebApplicationFactory()
            .ConfigureFakeUser(user =>
            {
                user.UserId = "custom-user-id";
                user.Name = "CustomUser";
                user.Email = "custom@example.com";
                user.TenantId = "custom-tenant";
            });
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/weatherforecast");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetWeatherForecast_ChangingUserMidTest_Works()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act 1 - First request with default user
        var response1 = await client.GetAsync("/weatherforecast");
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Change user mid-test
        factory.FakeUser.UserId = "changed-user-id";
        factory.FakeUser.Name = "ChangedUser";

        // Act 2 - Second request with changed user
        var response2 = await client.GetAsync("/weatherforecast");
        response2.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Change to unauthenticated
        factory.FakeUser.IsAuthenticated = false;

        // Act 3 - Third request should fail
        var response3 = await client.GetAsync("/weatherforecast");
        response3.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}