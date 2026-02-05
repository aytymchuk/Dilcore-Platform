using Dilcore.Authentication.Abstractions;
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
    public async Task AssignTenantOwnerAsync_ShouldAddOwnerRole_EvenWhenNotRegistered()
    {
        // Arrange
        var userId = Guid.CreateVersion7().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string tenantId = "owner-test-tenant";

        // Act - Call without registration
        await grain.AssignTenantOwnerAsync(tenantId);
        var tenants = await grain.GetTenantsAsync();
        var roles = await grain.GetTenantRolesAsync(tenantId);

        // Assert
        tenants.ShouldNotBeEmpty();
        tenants.Any(t => t.TenantId == tenantId).ShouldBeTrue();
        roles.ShouldContain(Roles.Owner);
    }

    [Test]
    public async Task AssignTenantOwnerAsync_ShouldBeIdempotent()
    {
        // Arrange
        var userId = Guid.CreateVersion7().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string tenantId = "idempotent-test-tenant";

        // Act
        await grain.AssignTenantOwnerAsync(tenantId);
        await grain.AssignTenantOwnerAsync(tenantId);

        var tenants = await grain.GetTenantsAsync();
        var roles = await grain.GetTenantRolesAsync(tenantId);

        // Assert
        tenants.Count.ShouldBe(1);
        roles.Count.ShouldBe(1);
        roles.ShouldContain(Roles.Owner);
    }

    [Test]
    public async Task AssignTenantOwnerAsync_ShouldSupportMultipleTenants()
    {
        // Arrange
        var userId = Guid.CreateVersion7().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string tenant1 = "tenant-1";
        const string tenant2 = "tenant-2";

        // Act
        await grain.AssignTenantOwnerAsync(tenant1);
        await grain.AssignTenantOwnerAsync(tenant2);

        var tenants = await grain.GetTenantsAsync();

        // Assert
        tenants.Count.ShouldBe(2);
        tenants.Any(t => t.TenantId == tenant1).ShouldBeTrue();
        tenants.Any(t => t.TenantId == tenant2).ShouldBeTrue();
    }

    [Test]
    public async Task AssignTenantOwnerAsync_ShouldThrowArgumentException_WhenTenantIdIsInvalid()
    {
        // Arrange
        var userId = Guid.CreateVersion7().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(() => grain.AssignTenantOwnerAsync(""));
        await Should.ThrowAsync<ArgumentException>(() => grain.AssignTenantOwnerAsync("   "));
    }

    [Test]
    public async Task GetTenantRolesAsync_ShouldReturnRoles_WhenTenantExists()
    {
        // Arrange
        var userId = Guid.CreateVersion7().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string tenantId = "roles-test-tenant";

        await grain.AssignTenantOwnerAsync(tenantId);

        // Act
        var result = await grain.GetTenantRolesAsync(tenantId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain(Roles.Owner);
    }

    [Test]
    public async Task GetTenantRolesAsync_ShouldReturnEmpty_WhenTenantDoesNotExist()
    {
        // Arrange
        var userId = Guid.CreateVersion7().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);

        await grain.RegisterAsync("no-tenant@example.com", "No", "Tenant");

        // Act
        var result = await grain.GetTenantRolesAsync("non-existent");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task IsRegisteredAsync_ShouldReturnCorrectStatus_BeforeAndAfterRegistration()
    {
        // Arrange
        var userId = Guid.CreateVersion7().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);

        // Act & Assert - Before
        var isRegisteredBefore = await grain.IsRegisteredAsync();
        isRegisteredBefore.ShouldBeFalse();

        // Act - Register
        await grain.RegisterAsync("status@example.com", "Status", "User");

        // Act & Assert - After
        var isRegisteredAfter = await grain.IsRegisteredAsync();
        isRegisteredAfter.ShouldBeTrue();
    }
}