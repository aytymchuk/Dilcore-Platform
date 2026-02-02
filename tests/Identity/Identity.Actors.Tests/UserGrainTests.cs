using Dilcore.Identity.Actors.Abstractions;
using Orleans.TestingHost;
using Shouldly;

namespace Dilcore.Identity.Actors.Tests;

/// <summary>
/// Integration tests for UserGrain using Orleans TestCluster.
/// </summary>
[TestFixture]
public class UserGrainTests
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
    public async Task RegisterAsync_ShouldCreateUser_WhenNotRegistered()
    {
        // Arrange
        var userId = Guid.CreateVersion7().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string email = "test@example.com";
        const string firstName = "Test";
        const string lastName = "User";

        // Act
        var beforeRegister = DateTime.UtcNow;
        var result = await grain.RegisterAsync(email, firstName, lastName);
        var afterRegister = DateTime.UtcNow;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.User.ShouldNotBeNull();

        result.User.Id.ShouldNotBe(Guid.Empty);
        result.User.Email.ShouldBe(email);
        result.User.FirstName.ShouldBe(firstName);
        result.User.LastName.ShouldBe(lastName);
        result.User.RegisteredAt.ShouldBeGreaterThanOrEqualTo(beforeRegister);
        result.User.RegisteredAt.ShouldBeLessThanOrEqualTo(afterRegister);
    }

    [Test]
    public async Task RegisterAsync_ShouldReturnFailure_WhenAlreadyRegistered()
    {
        // Arrange
        var userId = Guid.CreateVersion7().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string email = "existing@example.com";
        const string firstName = "Existing";
        const string lastName = "User";

        // First registration
        var firstResult = await grain.RegisterAsync(email, firstName, lastName);
        firstResult.IsSuccess.ShouldBeTrue();

        // Act - Try to register again with different data
        var secondResult = await grain.RegisterAsync("different@example.com", "Different", "Person");

        // Assert - Should return failure
        secondResult.ShouldNotBeNull();
        secondResult.IsSuccess.ShouldBeFalse();
        secondResult.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
        secondResult.ErrorMessage.ShouldContain("User is already registered");
    }

    [Test]
    public async Task GetProfileAsync_ShouldReturnNull_WhenUserNotRegistered()
    {
        // Arrange
        var userId = Guid.CreateVersion7().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);

        // Act
        var result = await grain.GetProfileAsync();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetProfileAsync_ShouldReturnUser_WhenRegistered()
    {
        // Arrange
        var userId = Guid.CreateVersion7().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string email = "profile@example.com";
        const string firstName = "Profile";
        const string lastName = "User";

        // Register first
        var registerResult = await grain.RegisterAsync(email, firstName, lastName);
        registerResult.IsSuccess.ShouldBeTrue();

        // Act
        var result = await grain.GetProfileAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Email.ShouldBe(email);
        result.FirstName.ShouldBe(firstName);
        result.LastName.ShouldBe(lastName);
    }

    [Test]
    public async Task UserState_ShouldBeAccessible_FromMultipleReferences()
    {
        // Arrange
        var userId = Guid.CreateVersion7().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string email = "persist@example.com";
        const string firstName = "Persist";
        const string lastName = "User";

        // Register
        var registerResult = await grain.RegisterAsync(email, firstName, lastName);
        registerResult.IsSuccess.ShouldBeTrue();

        // Act - Get a new reference and fetch profile
        var newGrainRef = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        var result = await newGrainRef.GetProfileAsync();

        // Assert - Data should persist
        result.ShouldNotBeNull();
        result.Email.ShouldBe(email);
    }

    [Test]
    public async Task AddTenantAsync_ShouldAddTenant_WhenUserRegistered()
    {
        // Arrange
        var userId = Guid.CreateVersion7().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string email = "tenant-test@example.com";
        const string firstName = "Tenant";
        const string lastName = "User";
        const string tenantId = "test-tenant-1";
        var roles = new[] { "Owner" };

        await grain.RegisterAsync(email, firstName, lastName);

        // Act
        await grain.AddTenantAsync(tenantId, roles);
        var tenants = await grain.GetTenantsAsync();

        // Assert
        tenants.ShouldNotBeNull();
        tenants.ShouldNotBeEmpty();
        var tenantAccess = tenants.FirstOrDefault(t => t.TenantId == tenantId);
        tenantAccess.ShouldNotBeNull();
        tenantAccess.Roles.ShouldContain("Owner");
    }

    [Test]
    public async Task AddTenantAsync_ShouldUpdateRoles_WhenTenantAlreadyExists()
    {
        // Arrange
        var userId = Guid.CreateVersion7().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string tenantId = "test-tenant-2";
        
        await grain.RegisterAsync("update-roles@example.com", "Update", "Roles");
        await grain.AddTenantAsync(tenantId, new[] { "Viewer" });

        // Act
        await grain.AddTenantAsync(tenantId, new[] { "Editor" });
        var tenants = await grain.GetTenantsAsync();

        // Assert
        var tenantAccess = tenants.First(t => t.TenantId == tenantId);
        tenantAccess.Roles.ShouldContain("Viewer");
        tenantAccess.Roles.ShouldContain("Editor");
    }
}