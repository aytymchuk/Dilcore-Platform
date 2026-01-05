using Dilcore.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Moq;
using Shouldly;

namespace Dilcore.MultiTenant.Http.Extensions.Tests;

[TestFixture]
public class HttpTenantContextProviderTests
{
    [Test]
    public void GetTenantContext_WhenTenantExists_ReturnsContext()
    {
        // Arrange
        var tenantInfo = new AppTenantInfo("t1", "t1", "T1") { StorageIdentifier = "db-01" };

        var mtContextMock = new Mock<IMultiTenantContext<AppTenantInfo>>();
        mtContextMock.Setup(x => x.TenantInfo).Returns(tenantInfo);

        var httpContext = new DefaultHttpContext();
        httpContext.Items[typeof(IMultiTenantContext)] = mtContextMock.Object;

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var provider = new HttpTenantContextProvider(httpContextAccessorMock.Object);

        // Act
        var result = provider.GetTenantContext();

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("T1");
        result.StorageIdentifier.ShouldBe("db-01");
    }

    [Test]
    public void GetTenantContext_WhenNoTenant_ReturnsNull()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var provider = new HttpTenantContextProvider(httpContextAccessorMock.Object);

        // Act
        var result = provider.GetTenantContext();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void GetTenantContext_WhenTenantInfoIsNull_ReturnsNull()
    {
        // Arrange
        var mtContextMock = new Mock<IMultiTenantContext<AppTenantInfo>>();
        mtContextMock.Setup(x => x.TenantInfo).Returns((AppTenantInfo?)null);

        var httpContext = new DefaultHttpContext();
        httpContext.Items[typeof(IMultiTenantContext)] = mtContextMock.Object;

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var provider = new HttpTenantContextProvider(httpContextAccessorMock.Object);

        // Act
        var result = provider.GetTenantContext();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void Priority_Returns100()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var provider = new HttpTenantContextProvider(httpContextAccessorMock.Object);

        // Act & Assert
        provider.Priority.ShouldBe(100);
    }
}