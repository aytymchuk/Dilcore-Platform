using Dilcore.Authentication.Abstractions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System.Security.Claims;

namespace Dilcore.Authentication.Auth0.Tests;

[TestFixture]
public class Auth0ClaimsTransformationTests
{
    private Auth0ClaimsTransformation _transformation = null!;
    private Mock<IAuth0UserService> _auth0UserServiceMock = null!;
    private Mock<ILogger<Auth0ClaimsTransformation>> _loggerMock = null!;
    private HybridCache _cache = null!;
    private ServiceProvider _serviceProvider = null!;
    private Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor> _httpContextAccessorMock = null!;
    private Auth0Settings _settings = null!;

    [SetUp]
    public void SetUp()
    {
        _auth0UserServiceMock = new Mock<IAuth0UserService>();
        _loggerMock = new Mock<ILogger<Auth0ClaimsTransformation>>();
        _httpContextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        _settings = new Auth0Settings { UserProfileCacheMinutes = 30 };

        // Create a real HybridCache instance for testing
        var services = new ServiceCollection();
        services.AddHybridCache();
        _serviceProvider = services.BuildServiceProvider();
        _cache = _serviceProvider.GetRequiredService<HybridCache>();

        _transformation = new Auth0ClaimsTransformation(
            _auth0UserServiceMock.Object,
            _cache,
            _httpContextAccessorMock.Object,
            _loggerMock.Object,
            _settings);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider?.Dispose();
    }

    [Test]
    public async Task TransformAsync_WithAuthorizationHeader_AddsEmailAndNameClaims()
    {
        // Arrange
        // 1. Setup Principal
        var claims = new[]
        {
            new Claim(UserConstants.SubjectClaimType, "user-123")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        // 2. Setup HttpContext with Auth Header
        var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer valid-token";
        context.Request.Scheme = "https";
        context.Request.Host = new Microsoft.AspNetCore.Http.HostString("localhost");
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // 3. Setup Auth0 Service to return profile
        var profile = new Auth0UserProfile
        {
            Email = "test@example.com",
            Name = "Test User"
        };
        _auth0UserServiceMock
            .Setup(x => x.GetUserProfileAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBe(principal); // Should be a clone

        var emailClaim = result.FindFirst(UserConstants.EmailClaimType);
        emailClaim.ShouldNotBeNull();
        emailClaim.Value.ShouldBe("test@example.com");
        emailClaim.Issuer.ShouldBe("https://localhost");

        var nameClaim = result.FindFirst(UserConstants.NameClaimType);
        nameClaim.ShouldNotBeNull();
        nameClaim.Value.ShouldBe("Test User");
        nameClaim.Issuer.ShouldBe("https://localhost");
    }

    [Test]
    public async Task TransformAsync_WithUnauthenticatedUser_ReturnsUnchangedPrincipal()
    {
        // Arrange
        var principal = new ClaimsPrincipal();

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.ShouldBe(principal);
    }

    [Test]
    public async Task TransformAsync_WithNoUserId_ReturnsUnchangedPrincipal()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Email, "test@example.com") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.ShouldBe(principal);
    }

    [Test]
    public async Task TransformAsync_WithBothEmailAndName_ReturnsUnchangedPrincipal()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(UserConstants.SubjectClaimType, "user-123"),
            new Claim(UserConstants.EmailClaimType, "test@example.com"),
            new Claim(UserConstants.NameClaimType, "Test User")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.ShouldBe(principal);
    }

    [Test]
    public async Task TransformAsync_WithNoHttpContext_ReturnsUnchangedPrincipal()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(UserConstants.SubjectClaimType, "user-123")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((Microsoft.AspNetCore.Http.HttpContext)null!);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.ShouldBe(principal);
    }

    [Test]
    public async Task TransformAsync_WithNoAuthorizationHeader_ReturnsUnchangedPrincipal()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(UserConstants.SubjectClaimType, "user-123")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.ShouldBe(principal);
    }
}
