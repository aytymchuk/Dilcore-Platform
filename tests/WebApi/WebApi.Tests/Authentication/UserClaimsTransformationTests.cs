using System.Security.Claims;
using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.WebApi.Authentication;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Dilcore.WebApi.Tests.Authentication;

[TestFixture]
public class UserClaimsTransformationTests
{
    private Mock<IClusterClient> _startClusterClientMock;
    private Mock<ITenantContextResolver> _tenantContextResolverMock;
    private Mock<ILogger<UserClaimsTransformation>> _loggerMock;
    private Mock<IUserGrain> _userGrainMock;
    private UserClaimsTransformation _sut;

    [SetUp]
    public void Setup()
    {
        _startClusterClientMock = new Mock<IClusterClient>();
        _tenantContextResolverMock = new Mock<ITenantContextResolver>();
        _loggerMock = new Mock<ILogger<UserClaimsTransformation>>();
        _userGrainMock = new Mock<IUserGrain>();

        _startClusterClientMock.Setup(x => x.GetGrain<IUserGrain>(It.IsAny<string>(), null))
            .Returns(_userGrainMock.Object);

        _userGrainMock.Setup(x => x.GetProfileAsync()).ReturnsAsync((UserResponse?)null);

        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

        _sut = new UserClaimsTransformation(_startClusterClientMock.Object, _tenantContextResolverMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task TransformAsync_ShouldReturnOriginalPrincipal_WhenUserNotAuthenticated()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var result = await _sut.TransformAsync(principal);

        result.ShouldBe(principal);
    }

    [Test]
    public async Task TransformAsync_ShouldAddTenantClaims_WhenUserHasTenants()
    {
        var userId = "test-user";
        var tenantId1 = "tenant1";
        var tenantId2 = "tenant2";

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId)], "TestAuth"));

        var tenantAccess = new List<TenantAccess>
        {
            new() { TenantId = tenantId1 },
            new() { TenantId = tenantId2 }
        };

        _userGrainMock.Setup(x => x.GetTenantsAsync()).ReturnsAsync(tenantAccess);

        var result = await _sut.TransformAsync(principal);

        result.HasClaim(c => c.Type == UserConstants.TenantsClaimType && c.Value == tenantId1).ShouldBeTrue();
        result.HasClaim(c => c.Type == UserConstants.TenantsClaimType && c.Value == tenantId2).ShouldBeTrue();
    }

    [Test]
    public async Task TransformAsync_ShouldSkip_WhenAlreadyTransformed()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "test-user")], "TestAuth");
        identity.AddClaim(new Claim("urn:dilcore:claims-transformed", "true"));
        var principal = new ClaimsPrincipal(identity);

        var result = await _sut.TransformAsync(principal);

        result.ShouldBe(principal);
        _startClusterClientMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task TransformAsync_ShouldReturnOriginalPrincipal_WhenExceptionOccurs()
    {
        var userId = "test-user";
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId)], "TestAuth"));

        _startClusterClientMock.Setup(x => x.GetGrain<IUserGrain>(userId, null))
            .Throws(new Exception("Cluster error"));

        var result = await _sut.TransformAsync(principal);

        result.ShouldBe(principal);
        // Verify LogTransformationError was called (extension method uses LogError internally)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error transforming claims for user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task TransformAsync_ShouldAddRoleClaims_WhenTenantContextIsActive()
    {
        var userId = "test-user";
        var tenantId = "active-tenant";
        var role = "admin";

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId)], "TestAuth"));

        var tenantAccess = new List<TenantAccess>
        {
            new() { TenantId = tenantId, Roles = new HashSet<string> { role } }
        };

        _userGrainMock.Setup(x => x.GetTenantsAsync()).ReturnsAsync(tenantAccess);

        // Mock the resolved tenant context
        var tenantContextMock = new Mock<ITenantContext>();
        tenantContextMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        tenantContextMock.Setup(x => x.Name).Returns(tenantId);

        ITenantContext? outTenantContext = tenantContextMock.Object;
        _tenantContextResolverMock.Setup(x => x.TryResolve(out outTenantContext)).Returns(true);

        var result = await _sut.TransformAsync(principal);

        result.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == role).ShouldBeTrue();
    }
}
