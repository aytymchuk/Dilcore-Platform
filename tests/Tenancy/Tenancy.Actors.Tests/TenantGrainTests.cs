using Dilcore.Tenancy.Actors.Abstractions;
using Orleans.TestingHost;
using Shouldly;

namespace Dilcore.Tenancy.Actors.Tests;

/// <summary>
/// Integration tests for TenantGrain using Orleans TestCluster.
/// </summary>
[TestFixture]
[NonParallelizable]
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
        var result = await grain.CreateAsync(new CreateTenantGrainCommand
        {
            DisplayName = displayName,
            Description = description
        });

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Tenant.ShouldNotBeNull();    
        result.Tenant.Name.ShouldBe(displayName);
        result.Tenant.SystemName.ShouldBe(tenantName);
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
        var firstResult = await grain.CreateAsync(new CreateTenantGrainCommand
        {
            DisplayName = displayName,
            Description = description
        });

        // Act - Try to create again with different data
        var secondResult = await grain.CreateAsync(new CreateTenantGrainCommand
        {
            DisplayName = "Different Name",
            Description = "Different description"
        });

        // Assert - Should fail and return an error indicating tenant already exists
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
        await grain.CreateAsync(new CreateTenantGrainCommand
        {
            DisplayName = displayName,
            Description = description
        });

        // Act
        var result = await grain.GetAsync();

        // Assert
        result.ShouldNotBeNull();
        result.SystemName.ShouldBe(tenantName);
        result.Name.ShouldBe(displayName);
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
        await grain.CreateAsync(new CreateTenantGrainCommand
        {
            DisplayName = displayName,
            Description = description
        });

        // Act - Fetch tenant using a new grain reference to verify persistence
        var newGrainRef = Cluster.GrainFactory.GetGrain<ITenantGrain>(tenantName);
        var result = await newGrainRef.GetAsync();

        // Assert - Data should persist
        result.ShouldNotBeNull();
        result.Name.ShouldBe(displayName);
    }
}
