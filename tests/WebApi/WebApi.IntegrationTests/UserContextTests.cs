using System.Net;
using System.Net.Http.Json;
using Dilcore.WebApi.IntegrationTests.Infrastructure;
using Shouldly;

namespace Dilcore.WebApi.IntegrationTests;

[TestFixture]
public class UserContextTests : IDisposable
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUpClient()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task UserInfo_WithAuthenticatedUser_ReturnsUserInformation()
    {
        // Arrange
        _factory.FakeUser.IsAuthenticated = true;
        _factory.FakeUser.UserId = "test-user-123";
        _factory.FakeUser.Email = "test@example.com";
        _factory.FakeUser.FirstName = "Test";
        _factory.FakeUser.LastName = "User";

        // Act
        var response = await _client.GetAsync("/test/user-info");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<UserInfoResponse>();
        content.ShouldNotBeNull();
        content.userId.ShouldBe("test-user-123");
        content.email.ShouldBe("test@example.com");
        // FullName is constructed from FirstName + LastName in MockAuthenticationHandler
        content.fullName.ShouldBe("Test User");
    }

    [Test]
    public async Task UserInfo_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        _factory.FakeUser.IsAuthenticated = false;

        // Act
        var response = await _client.GetAsync("/test/user-info");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task UserInfo_WithPartialUserData_ReturnsAvailableFields()
    {
        // Arrange - Create a fresh factory for this test to avoid state pollution
        _factory.Dispose();
        _factory = new CustomWebApplicationFactory();
        _client.Dispose();
        _client = _factory.CreateClient();

        _factory.FakeUser.IsAuthenticated = true;
        _factory.FakeUser.UserId = "partial-user-456";
        _factory.FakeUser.TenantId = "test-tenant";
        _factory.FakeUser.Email = null!; // No email
        _factory.FakeUser.FirstName = null; // No first name
        _factory.FakeUser.LastName = null; // No last name
        _factory.FakeUser.Name = null!; // No name either

        // Act
        var response = await _client.GetAsync("/test/user-info");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<UserInfoResponse>();
        content.ShouldNotBeNull();
        content.userId.ShouldBe("partial-user-456");
        content.email.ShouldBeNull();
        content.fullName.ShouldBeNull();
    }

    private record UserInfoResponse(string? userId, string? email, string? fullName);
}
