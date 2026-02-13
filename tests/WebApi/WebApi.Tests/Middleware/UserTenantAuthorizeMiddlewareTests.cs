using System.Security.Claims;
using Dilcore.Authentication.Abstractions;
using Dilcore.Authentication.Abstractions.Exceptions;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Dilcore.WebApi.Tests.Middleware;

[TestFixture]
public class UserTenantAuthorizeMiddlewareTests
{
    private Mock<ILogger<UserTenantAuthorizeMiddleware>> _loggerMock;
    private Mock<RequestDelegate> _nextMock;
    private Mock<ITenantContextResolver> _resolverMock;
    private UserTenantAuthorizeMiddleware _sut;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<UserTenantAuthorizeMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        _resolverMock = new Mock<ITenantContextResolver>();
        _sut = new UserTenantAuthorizeMiddleware(_loggerMock.Object, _resolverMock.Object);
    }

    private void SetEndpoint(HttpContext context)
    {
        var endpoint = new Endpoint(null, new EndpointMetadataCollection(), "TestEndpoint");
        context.SetEndpoint(endpoint);
    }

    [Test]
    public async Task InvokeAsync_ShouldCallNext_WhenNoTenantContext()
    {
        var context = new DefaultHttpContext();
        SetEndpoint(context);

        ITenantContext? nullContext = null;
        _resolverMock.Setup(x => x.TryResolve(out nullContext)).Returns(false);

        await _sut.InvokeAsync(context, _nextMock.Object);

        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Test]
    public async Task InvokeAsync_ShouldCallNext_WhenUserNotAuthenticated()
    {
        var context = new DefaultHttpContext();
        SetEndpoint(context);

        // Mock success but with valid context to pass first check
        var tenantContextMock = new Mock<ITenantContext>();
        tenantContextMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        tenantContextMock.Setup(x => x.StorageIdentifier).Returns("tenant1");
        ITenantContext? outContext = tenantContextMock.Object;
        _resolverMock.Setup(x => x.TryResolve(out outContext)).Returns(true);

        await _sut.InvokeAsync(context, _nextMock.Object);

        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Test]
    public async Task InvokeAsync_ShouldCallNext_WhenUserAuthorizedForTenant()
    {
        var tenantId = "tenant1";
        var context = new DefaultHttpContext();
        SetEndpoint(context);
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("tenants", tenantId)], "TestAuth"));
        context.User = user;

        var tenantContextMock = new Mock<ITenantContext>();
        tenantContextMock.Setup(x => x.Id).Returns(Guid.NewGuid()); // Ensure ID is not empty
        tenantContextMock.Setup(x => x.StorageIdentifier).Returns(tenantId);
        tenantContextMock.Setup(x => x.Name).Returns(tenantId);

        ITenantContext? outContext = tenantContextMock.Object;
        _resolverMock.Setup(x => x.TryResolve(out outContext)).Returns(true);

        await _sut.InvokeAsync(context, _nextMock.Object);

        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Test]
    public async Task InvokeAsync_ShouldReturnForbidden_WhenUserNotAuthorizedForTenant()
    {
        var tenantId = "tenant1";
        var context = new DefaultHttpContext();
        SetEndpoint(context);
        var identity = new ClaimsIdentity(new[] { new Claim(UserConstants.TenantsClaimType, "other-tenant") }, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        context.User = user;

        var tenantContextMock = new Mock<ITenantContext>();
        tenantContextMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        tenantContextMock.Setup(x => x.StorageIdentifier).Returns(tenantId);
        tenantContextMock.Setup(x => x.Name).Returns("tenant1");

        ITenantContext? outContext = tenantContextMock.Object;
        _resolverMock.Setup(x => x.TryResolve(out outContext)).Returns(true);

        // Act & Assert
        var ex = await Should.ThrowAsync<ForbiddenException>(() => _sut.InvokeAsync(context, _nextMock.Object));
        ex.Message.ShouldBe("Access to tenant is forbidden.");

        _nextMock.Verify(x => x(context), Times.Never);
    }
}
