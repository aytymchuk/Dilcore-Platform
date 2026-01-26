using System.Net;
using Dilcore.Identity.Contracts.Profile;
using Dilcore.Identity.Contracts.Register;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApi.IntegrationTests.Infrastructure;
using Refit;
using Shouldly;

namespace Dilcore.WebApi.IntegrationTests;

/// <summary>
/// Integration tests for User endpoints (/users).
/// </summary>
[TestFixture]
public class UserEndpointTests : BaseIntegrationTest
{
    private IDisposableClient<IIdentityClient> _identityClient = null!;
    private const string TenantId = "test-tenant";

    [SetUp]
    public async Task SetUpClient()
    {
        await SeedTenantAsync(Factory, TenantId);

        // Reset the fake user to defaults before each test
        Factory.FakeUser.UserId = $"test-user-{Guid.NewGuid():N}";
        Factory.FakeUser.Email = "test@example.com";
        Factory.FakeUser.FirstName = "Test";
        Factory.FakeUser.LastName = "User";
        Factory.FakeUser.TenantId = TenantId;
        Factory.FakeUser.IsAuthenticated = true;

        _identityClient = Factory.CreateIdentityClient(TenantId);
    }

    [TearDown]
    public void TearDownClient()
    {
        _identityClient.Dispose();
    }

    #region POST /users/register

    [Test]
    public async Task RegisterUser_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        var request = new RegisterUserDto("new.user@example.com", "New", "User");

        // Act
        var result = await _identityClient.Client.RegisterUserAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBe("new.user@example.com");
        result.FirstName.ShouldBe("New");
        result.LastName.ShouldBe("User");
        result.Id.ShouldNotBe(Guid.Empty);
    }

    [Test]
    public async Task RegisterUser_ShouldReturnConflict_WhenUserAlreadyRegistered()
    {
        // Arrange - use a fixed user ID for re-registration
        Factory.FakeUser.UserId = "existing-user-id";
        var request = new RegisterUserDto("existing@example.com", "Existing", "User");

        // First registration
        await _identityClient.Client.RegisterUserAsync(request);

        // Act & Assert - second registration with same user ID should return Conflict
        var exception = await Should.ThrowAsync<ApiException>(() => _identityClient.Client.RegisterUserAsync(request));
        exception.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task RegisterUser_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        Factory.FakeUser.IsAuthenticated = false;
        var request = new RegisterUserDto("test@example.com", "Test", "User");

        // Act & Assert
        var exception = await Should.ThrowAsync<ApiException>(() => _identityClient.Client.RegisterUserAsync(request));
        exception.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /users/me

    [Test]
    public async Task GetCurrentUser_ShouldReturnOk_WhenUserExists()
    {
        // Arrange - first register the user
        var request = new RegisterUserDto("me@example.com", "Me", "User");
        await _identityClient.Client.RegisterUserAsync(request);

        // Act
        var result = await _identityClient.Client.GetCurrentUserAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBe("me@example.com");
        result.FirstName.ShouldBe("Me");
        result.LastName.ShouldBe("User");
    }

    [Test]
    public async Task GetCurrentUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange - use a new user ID that hasn't been registered
        Factory.FakeUser.UserId = $"nonexistent-{Guid.NewGuid():N}";

        // Act & Assert
        var exception = await Should.ThrowAsync<ApiException>(() => _identityClient.Client.GetCurrentUserAsync());
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetCurrentUser_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        Factory.FakeUser.IsAuthenticated = false;

        // Act & Assert
        var exception = await Should.ThrowAsync<ApiException>(() => _identityClient.Client.GetCurrentUserAsync());
        exception.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion
}
