using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.Orleans.Extensions.Tests.TestGrains;
using Orleans.TestingHost;
using Shouldly;

namespace Dilcore.MultiTenant.Orleans.Extensions.Tests;

/// <summary>
/// Integration tests for tenant context propagation through Orleans grain calls.
/// </summary>
[TestFixture]
public class TenantContextIntegrationTests
{
    private ClusterFixture _fixture = null!;
    private TestCluster Cluster => _fixture.Cluster;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _fixture = new ClusterFixture();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Cluster.StopAllSilos();
        Cluster.Dispose();
        _fixture.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        // Clear RequestContext before each test
        RequestContext.Clear();
    }

    [Test]
    public async Task Grain_ShouldReceiveTenantContext_WhenSetInRequestContext()
    {
        // Arrange
        var grainId = Guid.NewGuid().ToString();
        var grain = Cluster.GrainFactory.GetGrain<ITenantTestGrain>(grainId);

        // Set tenant context in RequestContext (simulating outgoing filter behavior)
        OrleansTenantContextAccessor.SetTenantContext(
            new TenantContext("test-tenant", "storage-123"));

        // Act
        var tenantName = await grain.GetCurrentTenantNameAsync();
        var storageId = await grain.GetCurrentTenantStorageIdentifierAsync();

        // Assert
        tenantName.ShouldBe("test-tenant");
        storageId.ShouldBe("storage-123");
    }

    [Test]
    public async Task Grain_ShouldReceiveNull_WhenNoTenantContextSet()
    {
        // Arrange
        var grainId = Guid.NewGuid().ToString();
        var grain = Cluster.GrainFactory.GetGrain<ITenantTestGrain>(grainId);

        // Act
        var tenantName = await grain.GetCurrentTenantNameAsync();

        // Assert
        tenantName.ShouldBeNull();
    }

    [Test]
    public async Task TenantContext_ShouldPropagateAcrossGrainCalls()
    {
        // Arrange
        var grainId1 = Guid.NewGuid().ToString();
        var grainId2 = Guid.NewGuid().ToString();
        var grain1 = Cluster.GrainFactory.GetGrain<ITenantTestGrain>(grainId1);

        // Set tenant context in RequestContext
        OrleansTenantContextAccessor.SetTenantContext(
            new TenantContext("propagated-tenant", "prop-storage-456"));

        // Act - grain1 calls grain2 and gets its tenant name
        var tenantNameFromGrain2 = await grain1.CallAnotherGrainAndGetTenantNameAsync(grainId2);

        // Assert - grain2 should see the same tenant context
        tenantNameFromGrain2.ShouldBe("propagated-tenant");
    }

    [Test]
    public async Task TenantContext_ShouldIsolateAcrossDifferentCalls()
    {
        // Arrange
        var grainId = Guid.NewGuid().ToString();
        var grain = Cluster.GrainFactory.GetGrain<ITenantTestGrain>(grainId);

        // Act & Assert - First call with tenant A
        OrleansTenantContextAccessor.SetTenantContext(
            new TenantContext("tenant-a", "storage-a"));
        var tenantNameA = await grain.GetCurrentTenantNameAsync();
        tenantNameA.ShouldBe("tenant-a");

        // Clear and set different tenant context
        RequestContext.Clear();
        OrleansTenantContextAccessor.SetTenantContext(
            new TenantContext("tenant-b", "storage-b"));
        var tenantNameB = await grain.GetCurrentTenantNameAsync();
        tenantNameB.ShouldBe("tenant-b");

        // Verify tenant B is different from tenant A
        tenantNameB.ShouldNotBe(tenantNameA);
    }

    [Test]
    public async Task TenantContext_WithOnlyName_ShouldPropagate()
    {
        // Arrange
        var grainId = Guid.NewGuid().ToString();
        var grain = Cluster.GrainFactory.GetGrain<ITenantTestGrain>(grainId);

        // Set tenant context with only name
        OrleansTenantContextAccessor.SetTenantContext(
            new TenantContext("name-only-tenant", null));

        // Act
        var tenantName = await grain.GetCurrentTenantNameAsync();
        var storageId = await grain.GetCurrentTenantStorageIdentifierAsync();

        // Assert
        tenantName.ShouldBe("name-only-tenant");
        storageId.ShouldBeNull();
    }

    [Test]
    public async Task TenantContext_WithOnlyStorageIdentifier_ShouldPropagate()
    {
        // Arrange
        var grainId = Guid.NewGuid().ToString();
        var grain = Cluster.GrainFactory.GetGrain<ITenantTestGrain>(grainId);

        // Set tenant context with only storage identifier
        OrleansTenantContextAccessor.SetTenantContext(
            new TenantContext(null, "storage-only-789"));

        // Act
        var tenantName = await grain.GetCurrentTenantNameAsync();
        var storageId = await grain.GetCurrentTenantStorageIdentifierAsync();

        // Assert
        tenantName.ShouldBeNull();
        storageId.ShouldBe("storage-only-789");
    }
}
