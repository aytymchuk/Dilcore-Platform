using System.Security.Claims;
using Dilcore.Authentication.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Dilcore.Authentication.Auth0.Tests;

[TestFixture]
public class Auth0ClaimsTransformationTests
{
    private Mock<IAuth0UserService> _auth0UserServiceMock = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
    private Mock<ILogger<Auth0ClaimsTransformation>> _loggerMock = null!;
    private Auth0Settings _settings = null!;

    [SetUp]
    public void SetUp()
    {
        _auth0UserServiceMock = new Mock<IAuth0UserService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<Auth0ClaimsTransformation>>();
        _settings = new Auth0Settings
        {
            Domain = "test.auth0.com",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            Audience = "test-audience",
            UserProfileCacheMinutes = 30
        };
    }

    [Test]
    public async Task TransformAsync_WithUnauthenticatedPrincipal_ReturnsSamePrincipal()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity()); // No authentication type
        var cache = CreateHybridCache();
        var transformation = new Auth0ClaimsTransformation(
            _auth0UserServiceMock.Object,
            cache,
            _httpContextAccessorMock.Object,
            _loggerMock.Object,
            _settings);

        // Act
        var result = await transformation.TransformAsync(principal);

        // Assert
        result.ShouldBe(principal);
        _auth0UserServiceMock.Verify(x => x.GetUserProfileAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task TransformAsync_WithNoUserId_ReturnsSamePrincipal()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var cache = CreateHybridCache();
        var transformation = new Auth0ClaimsTransformation(
            _auth0UserServiceMock.Object,
            cache,
            _httpContextAccessorMock.Object,
            _loggerMock.Object,
            _settings);

        // Act
        var result = await transformation.TransformAsync(principal);

        // Assert
        result.ShouldBe(principal);
        _auth0UserServiceMock.Verify(x => x.GetUserProfileAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task TransformAsync_WithBothEmailAndName_ReturnsSamePrincipal()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(UserConstants.EmailClaimType, "test@example.com"),
            new Claim(UserConstants.NameClaimType, "Test User")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var cache = CreateHybridCache();
        var transformation = new Auth0ClaimsTransformation(
            _auth0UserServiceMock.Object,
            cache,
            _httpContextAccessorMock.Object,
            _loggerMock.Object,
            _settings);

        // Act
        var result = await transformation.TransformAsync(principal);

        // Assert
        result.ShouldBe(principal);
        _auth0UserServiceMock.Verify(x => x.GetUserProfileAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task TransformAsync_WithStandardClaimTypes_AlsoSkipsEnrichment()
    {
        // Arrange - Using standard ClaimTypes instead of UserConstants
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "Test User")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var cache = CreateHybridCache();
        var transformation = new Auth0ClaimsTransformation(
            _auth0UserServiceMock.Object,
            cache,
            _httpContextAccessorMock.Object,
            _loggerMock.Object,
            _settings);

        // Act
        var result = await transformation.TransformAsync(principal);

        // Assert
        result.ShouldBe(principal);
        _auth0UserServiceMock.Verify(x => x.GetUserProfileAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task TransformAsync_UsesSubClaimAsPriority()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(UserConstants.SubjectClaimType, "sub-user"),
            new Claim(UserConstants.UserIdClaimType, "uid-user"),
            new Claim(ClaimTypes.NameIdentifier, "nameidentifier-user"),
            new Claim(UserConstants.EmailClaimType, "test@example.com"),
            new Claim(UserConstants.NameClaimType, "Test User")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var cache = CreateHybridCache();
        var transformation = new Auth0ClaimsTransformation(
            _auth0UserServiceMock.Object,
            cache,
            _httpContextAccessorMock.Object,
            _loggerMock.Object,
            _settings);

        // Act
        var result = await transformation.TransformAsync(principal);

        // Assert
        result.ShouldBe(principal);
        // The test verifies that the transformation doesn't fail with multiple ID claims
    }

    private static HybridCache CreateHybridCache()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<HybridCache>();
    }
}
