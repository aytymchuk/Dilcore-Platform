using Dilcore.Identity.Contracts.Profile;
using Dilcore.Identity.Contracts.Register;
using Dilcore.WebApi.Client.Clients;
using RichardSzalay.MockHttp;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace Dilcore.WebApi.Client.Tests;

[TestFixture]
public class IdentityClientTests
{
    private MockHttpMessageHandler _mockHttp = null!;
    private IIdentityClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClient = _mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.example.com");

        _client = Refit.RestService.For<IIdentityClient>(httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _mockHttp.Dispose();
    }

    [Test]
    public async Task RegisterUserAsync_ShouldReturnUserDto_WhenSuccessful()
    {
        // Arrange
        var registerDto = new RegisterUserDto("test@example.com", "Test", "User");
        var expectedUser = new UserDto(
            Guid.NewGuid(),
            "test@example.com",
            "Test",
            "User",
            DateTime.UtcNow);

        _mockHttp.When(HttpMethod.Post, "https://api.example.com/users/register")
            .Respond("application/json", JsonSerializer.Serialize(expectedUser));

        // Act
        var result = await _client.RegisterUserAsync(registerDto);

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBe("test@example.com");
        result.FirstName.ShouldBe("Test");
        result.LastName.ShouldBe("User");
    }

    [Test]
    public void RegisterUserAsync_ShouldThrowException_WhenConflict()
    {
        // Arrange
        var registerDto = new RegisterUserDto("existing@example.com", "Existing", "User");

        _mockHttp.When(HttpMethod.Post, "https://api.example.com/users/register")
            .Respond(HttpStatusCode.Conflict);

        // Act & Assert
        Should.Throw<Refit.ApiException>(async () => await _client.RegisterUserAsync(registerDto));
    }

    [Test]
    public async Task GetCurrentUserAsync_ShouldReturnUserDto_WhenExists()
    {
        // Arrange
        var expectedUser = new UserDto(
            Guid.NewGuid(),
            "current@example.com",
            "Current",
            "User",
            DateTime.UtcNow);

        _mockHttp.When(HttpMethod.Get, "https://api.example.com/users/me")
            .Respond("application/json", JsonSerializer.Serialize(expectedUser));

        // Act
        var result = await _client.GetCurrentUserAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBe("current@example.com");
        result.FirstName.ShouldBe("Current");
        result.LastName.ShouldBe("User");
    }

    [Test]
    public void GetCurrentUserAsync_ShouldThrowException_WhenNotFound()
    {
        // Arrange
        _mockHttp.When(HttpMethod.Get, "https://api.example.com/users/me")
            .Respond(HttpStatusCode.NotFound);

        // Act & Assert
        Should.Throw<Refit.ApiException>(async () => await _client.GetCurrentUserAsync());
    }
}
