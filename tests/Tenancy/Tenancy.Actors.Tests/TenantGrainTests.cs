using Dilcore.Tenancy.Actors.Abstractions;
using Orleans.TestingHost;
using Shouldly;

namespace Dilcore.Tenancy.Actors.Tests;

/// <summary>
/// Integration tests for TenantGrain using Orleans TestCluster.
/// </summary>
[TestFixture]
public class TenantGrainTests
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
        _fixture.Dispose();
    }

    [Test]
    public async Task CreateAsync_ShouldCreateTenant_WhenNotExists()
    {
        // Arrange
        var tenantName = $"test-tenant-{Guid.NewGuid():N}";
        var grain = Cluster.GrainFactory.GetGrain<ITenantGrain>(tenantName);
        const string displayName = "Test Tenant";
        const string description = "A test tenant for unit testing";

        // Act
        var result = await grain.CreateAsync(displayName, description);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Tenant.ShouldNotBeNull();
        result.Tenant.Name.ShouldBe(tenantName);
        result.Tenant.DisplayName.ShouldBe(displayName);
        result.Tenant.Description.ShouldBe(description);
        result.Tenant.CreatedAt.ShouldBeGreaterThan(DateTime.MinValue);
    }

    [Test]
    public async Task CreateAsync_ShouldReturnExisting_WhenAlreadyCreated()
    {
        // Arrange
        var tenantName = $"existing-tenant-{Guid.NewGuid():N}";
        var grain = Cluster.GrainFactory.GetGrain<ITenantGrain>(tenantName);
        const string displayName = "Existing Tenant";
        const string description = "Original description";

        // First creation
        var firstResult = await grain.CreateAsync(displayName, description);

        // Act - Try to create again with different data
        var secondResult = await grain.CreateAsync("Different Name", "Different description");

        // Assert - Should return original data
        secondResult.ShouldNotBeNull();
        secondResult.IsSuccess.ShouldBeFalse();
        secondResult.ErrorMessage.ShouldNotBeNull();
        secondResult.ErrorMessage.ShouldContain("already exists");
    }

    [Test]
    public async Task GetAsync_ShouldReturnNull_WhenTenantNotExists()
    {
        // Arrange
        var tenantName = $"nonexistent-{Guid.NewGuid():N}";
        var grain = Cluster.GrainFactory.GetGrain<ITenantGrain>(tenantName);

        // Act
        var result = await grain.GetAsync();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetAsync_ShouldReturnTenant_WhenExists()
    {
        // Arrange
        var tenantName = $"get-tenant-{Guid.NewGuid():N}";
        var grain = Cluster.GrainFactory.GetGrain<ITenantGrain>(tenantName);
        const string displayName = "Get Tenant";
        const string description = "Tenant for get test";

        // Create first
        await grain.CreateAsync(displayName, description);

        // Act
        var result = await grain.GetAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(tenantName);
        result.DisplayName.ShouldBe(displayName);
        result.Description.ShouldBe(description);
    }

    [Test]
    public async Task TenantState_ShouldPersist_AcrossGrainCalls()
    {
        // Arrange
        var tenantName = $"persist-tenant-{Guid.NewGuid():N}";
        var grain = Cluster.GrainFactory.GetGrain<ITenantGrain>(tenantName);
        const string displayName = "Persist Tenant";
        const string description = "Tenant for persistence test";

        // Create
        await grain.CreateAsync(displayName, description);

        // Act - Fetch tenant using a new grain reference to verify persistence
        var newGrainRef = Cluster.GrainFactory.GetGrain<ITenantGrain>(tenantName);
        var result = await newGrainRef.GetAsync();

        // Assert - Data should persist
        result.ShouldNotBeNull();
        result.DisplayName.ShouldBe(displayName);
    }
}
