using System.Diagnostics;
using Dilcore.MultiTenant.Abstractions;

using Dilcore.MultiTenant.Http.Extensions.Telemetry;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Dilcore.MultiTenant.Http.Extensions.Tests.Telemetry;

[TestFixture]
public class TenantTelemetryEnricherTests
{
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<ITenantContextResolver> _tenantResolverMock;
    private DefaultHttpContext _httpContext;
    private TenantTelemetryEnricher _enricher;

    [SetUp]
    public void Setup()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _tenantResolverMock = new Mock<ITenantContextResolver>();
        _httpContext = new DefaultHttpContext { RequestServices = _serviceProviderMock.Object };
        _enricher = new TenantTelemetryEnricher();

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(ITenantContextResolver)))
            .Returns(_tenantResolverMock.Object);
    }

    [Test]
    public void Enrich_ShouldSetTenantIdTag_WhenTenantIsResolved()
    {
        // Arrange
        var activity = new Activity("TestActivity");
        var tenantContext = new TenantContext("test-tenant", "db1");
        _tenantResolverMock.Setup(r => r.Resolve()).Returns(tenantContext);

        // Act
        _enricher.Enrich(activity, _httpContext.Request);

        // Assert
        activity.Tags.ShouldContain(t => t.Key == "tenant.id" && t.Value == "test-tenant");
    }

    [Test]
    public void Enrich_ShouldNotSetTag_WhenTenantContextIsNull()
    {
        // Arrange
        var activity = new Activity("TestActivity");
        _tenantResolverMock.Setup(r => r.Resolve()).Returns((ITenantContext)null);

        // Act
        _enricher.Enrich(activity, _httpContext.Request);

        // Assert
        activity.Tags.ShouldNotContain(t => t.Key == "tenant.id");
    }

    [Test]
    public void Enrich_ShouldNotSetTag_WhenTenantNotResolvedExceptionIsThrown()
    {
        // Arrange
        var activity = new Activity("TestActivity");
        _tenantResolverMock.Setup(r => r.Resolve()).Throws(new TenantNotResolvedException());

        // Act
        _enricher.Enrich(activity, _httpContext.Request);

        // Assert
        activity.Tags.ShouldNotContain(t => t.Key == "tenant.id");
    }
}
