using System.Net;
using System.Net.Http.Json;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.WebApi.IntegrationTests.Infrastructure;
using Shouldly;

namespace Dilcore.WebApi.IntegrationTests;

/// <summary>
/// Integration tests for User endpoints (/users).
/// </summary>
[TestFixture]
public class UserEndpointTests : BaseIntegrationTest
{
    private HttpClient _client = null!;
    private const string TenantId = "test-tenant";

    [SetUp]
    public async Task SetUpClient()
    {
        await SeedTenantAsync(Factory, TenantId);

        // Reset the fake user to defaults before each test
        Factory.FakeUser.UserId = $"test-user-{Guid.NewGuid():N}";
        Factory.FakeUser.Email = "test@example.com";
        Factory.FakeUser.FullName = "Test User";
        Factory.FakeUser.TenantId = TenantId;
        Factory.FakeUser.IsAuthenticated = true;

        _client = Factory.CreateClient();
        // Set default tenant header for all requests
        _client.DefaultRequestHeaders.Add(TenantConstants.HeaderName, TenantId);
    }

    [TearDown]
    public void TearDownClient()
    {
        _client.Dispose();
    }



    #region POST /users/register

    [Test]
    public async Task RegisterUser_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        var command = new { Email = "new.user@example.com", FullName = "New User" };

        // Act
        var response = await _client.PostAsJsonAsync("/users/register", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.ShouldNotBeNull();
        result.Email.ShouldBe("new.user@example.com");
        result.FullName.ShouldBe("New User");
        result.Id.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public async Task RegisterUser_IsIdempotent_ReturnsExistingUserOnReRegistration()
    {
        // Arrange - use a fixed user ID for re-registration
        Factory.FakeUser.UserId = "existing-user-id";
        var command = new { Email = "existing@example.com", FullName = "Existing User" };

        // First registration
        var firstResponse = await _client.PostAsJsonAsync("/users/register", command);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var firstResult = await firstResponse.Content.ReadFromJsonAsync<UserDto>();
        firstResult.ShouldNotBeNull();

        // Act - second registration with same user ID returns existing user (idempotent)
        var secondResponse = await _client.PostAsJsonAsync("/users/register", command);

        // Assert - idempotent behavior: returns OK with existing user data
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var secondResult = await secondResponse.Content.ReadFromJsonAsync<UserDto>();
        secondResult.ShouldNotBeNull();
        secondResult.Id.ShouldBe(firstResult.Id);
        secondResult.Email.ShouldBe(firstResult.Email);
    }

    [Test]
    public async Task RegisterUser_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        Factory.FakeUser.IsAuthenticated = false;
        var command = new { Email = "test@example.com", FullName = "Test User" };

        // Act
        var response = await _client.PostAsJsonAsync("/users/register", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /users/me

    [Test]
    public async Task GetCurrentUser_ShouldReturnOk_WhenUserExists()
    {
        // Arrange - first register the user
        var registerCommand = new { Email = "me@example.com", FullName = "Me User" };
        await _client.PostAsJsonAsync("/users/register", registerCommand);

        // Act
        var response = await _client.GetAsync("/users/me");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.ShouldNotBeNull();
        result.Email.ShouldBe("me@example.com");
        result.FullName.ShouldBe("Me User");
    }

    [Test]
    public async Task GetCurrentUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange - use a new user ID that hasn't been registered
        Factory.FakeUser.UserId = $"nonexistent-{Guid.NewGuid():N}";

        // Act
        var response = await _client.GetAsync("/users/me");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetCurrentUser_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        Factory.FakeUser.IsAuthenticated = false;

        // Act
        var response = await _client.GetAsync("/users/me");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion
}
