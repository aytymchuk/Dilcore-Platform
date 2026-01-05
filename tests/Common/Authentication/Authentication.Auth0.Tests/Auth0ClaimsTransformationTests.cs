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
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddHybridCache();
        var serviceProvider = services.BuildServiceProvider();
        _cache = serviceProvider.GetRequiredService<HybridCache>();

        _transformation = new Auth0ClaimsTransformation(
            _auth0UserServiceMock.Object,
            _cache,
            _httpContextAccessorMock.Object,
            _loggerMock.Object,
            _settings);
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
