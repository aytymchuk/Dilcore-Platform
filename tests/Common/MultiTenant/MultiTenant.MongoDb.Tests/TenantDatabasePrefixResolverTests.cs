using Dilcore.MongoDB.Abstractions.Namespace;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.MongoDb;
using FluentAssertions;
using Moq;

namespace Dilcore.MultiTenant.MongoDb.Tests;

[TestFixture]
public class TenantDatabasePrefixResolverTests
{
    [Test]
    public async Task ResolveAsync_WhenTenantIsResolved_ReturnsStorageIdentifier()
    {
        var tenant = new Mock<ITenantContext>();
        tenant.Setup(x => x.StorageIdentifier).Returns("tenant-a");
        ITenantContext? resolved = tenant.Object;
        var tenantContextResolver = new Mock<ITenantContextResolver>();
        tenantContextResolver.Setup(x => x.TryResolve(out resolved)).Returns(true);

        var sut = new TenantDatabasePrefixResolver(tenantContextResolver.Object);
        var result = await sut.ResolveAsync(new NamespaceResolutionRequest
        {
            LogicalName = "Blueprints",
            Target = NamespaceTarget.Database
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("tenant-a");
    }

    [Test]
    public async Task ResolveAsync_WhenTenantIsMissing_ReturnsFailure()
    {
        ITenantContext? resolved = null;
        var tenantContextResolver = new Mock<ITenantContextResolver>();
        tenantContextResolver.Setup(x => x.TryResolve(out resolved)).Returns(false);

        var sut = new TenantDatabasePrefixResolver(tenantContextResolver.Object);
        var result = await sut.ResolveAsync(new NamespaceResolutionRequest
        {
            LogicalName = "Blueprints",
            Target = NamespaceTarget.Database
        });

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task ResolveAsync_WhenStorageIdentifierIsEmpty_ReturnsFailure()
    {
        var tenant = new Mock<ITenantContext>();
        tenant.Setup(x => x.StorageIdentifier).Returns(" ");
        ITenantContext? resolved = tenant.Object;
        var tenantContextResolver = new Mock<ITenantContextResolver>();
        tenantContextResolver.Setup(x => x.TryResolve(out resolved)).Returns(true);

        var sut = new TenantDatabasePrefixResolver(tenantContextResolver.Object);
        var result = await sut.ResolveAsync(new NamespaceResolutionRequest
        {
            LogicalName = "Blueprints",
            Target = NamespaceTarget.Database
        });

        result.IsFailed.Should().BeTrue();
    }
}
