using System.Security.Claims;
using Dilcore.Authentication.Abstractions;
using Microsoft.AspNetCore.Http;
using Moq;
using Shouldly;

namespace Dilcore.Authentication.Http.Extensions.Tests;

[TestFixture]
public class HttpUserContextProviderTests
{
    private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
    private HttpUserContextProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _provider = new HttpUserContextProvider(_httpContextAccessorMock.Object);
    }

    [Test]
    public void Priority_ShouldBe100()
    {
        // Assert
        _provider.Priority.ShouldBe(100);
    }

    [Test]
    public void GetUserContext_WithNoHttpContext_ReturnsNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _provider.GetUserContext();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void GetUserContext_WithUnauthenticatedUser_ReturnsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // No authentication type
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _provider.GetUserContext();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void GetUserContext_WithNoUserId_ReturnsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "Test User")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _provider.GetUserContext();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void GetUserContext_WithSubClaim_ReturnsUserContext()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(UserConstants.SubjectClaimType, "user-123"),
            new Claim(UserConstants.EmailClaimType, "test@example.com"),
            new Claim(UserConstants.NameClaimType, "Test User")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _provider.GetUserContext();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe("user-123");
        result.Email.ShouldBe("test@example.com");
        result.FullName.ShouldBe("Test User");
    }

    [Test]
    public void GetUserContext_WithUidClaim_ReturnsUserContext()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(UserConstants.UserIdClaimType, "user-456"),
            new Claim(ClaimTypes.Email, "test2@example.com"),
            new Claim(ClaimTypes.Name, "Test User 2")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _provider.GetUserContext();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe("user-456");
        result.Email.ShouldBe("test2@example.com");
        result.FullName.ShouldBe("Test User 2");
    }

    [Test]
    public void GetUserContext_WithNameIdentifierClaim_ReturnsUserContext()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-789"),
            new Claim(ClaimTypes.Email, "test3@example.com"),
            new Claim(ClaimTypes.Name, "Test User 3")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _provider.GetUserContext();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe("user-789");
        result.Email.ShouldBe("test3@example.com");
        result.FullName.ShouldBe("Test User 3");
    }

    [Test]
    public void GetUserContext_WithOnlyUserId_ReturnsUserContextWithNullEmailAndFullName()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-only")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _provider.GetUserContext();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe("user-only");
        result.Email.ShouldBeNull();
        result.FullName.ShouldBeNull();
    }

    [Test]
    public void GetUserContext_PrioritizesSubOverUidOverNameIdentifier()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(UserConstants.SubjectClaimType, "sub-user"),
            new Claim(UserConstants.UserIdClaimType, "uid-user"),
            new Claim(ClaimTypes.NameIdentifier, "nameidentifier-user")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _provider.GetUserContext();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe("sub-user"); // Should use 'sub' claim first
    }

    [Test]
    public void GetUserContext_PrioritizesCustomEmailOverClaimsTypeEmail()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(UserConstants.EmailClaimType, "custom@example.com"),
            new Claim(ClaimTypes.Email, "standard@example.com")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _provider.GetUserContext();

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBe("custom@example.com"); // Should use custom email claim first
    }

    [Test]
    public void GetUserContext_PrioritizesCustomNameOverClaimsTypeName()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(UserConstants.NameClaimType, "Custom Name"),
            new Claim(ClaimTypes.Name, "Standard Name")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _provider.GetUserContext();

        // Assert
        result.ShouldNotBeNull();
        result.FullName.ShouldBe("Custom Name"); // Should use custom name claim first
    }
}
