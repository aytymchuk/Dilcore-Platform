using System.Net;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Contracts.Tenants.Create;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApi.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Shouldly;

namespace Dilcore.WebApi.IntegrationTests;

/// <summary>
/// Integration tests for Tenant endpoints (/tenants).
/// </summary>
[TestFixture]
public class TenantEndpointTests
{
    private CustomWebApplicationFactory _factory = null!;
    private IDisposableClient<ITenancyClient> _tenancyClient = null!;

    [OneTimeSetUp]
    public void SetUpFactory()
    {
        _factory = new CustomWebApplicationFactory();
    }

    [SetUp]
    public async Task SetUpClient()
    {
        // Reset the fake user to defaults before each test
        _factory.FakeUser.UserId = $"test-user-{Guid.CreateVersion7():N}";
        _factory.FakeUser.TenantId = $"test-tenant-{Guid.CreateVersion7():N}";
        _factory.FakeUser.IsAuthenticated = true;
        _tenancyClient = _factory.CreateTypedClient<ITenancyClient>();

        // Register the user to ensure happy paths pass
        using var scope = _factory.Services.CreateScope();

        var grainFactory = scope.ServiceProvider.GetRequiredService<IGrainFactory>();
        var userGrain = grainFactory.GetGrain<IUserGrain>(_factory.FakeUser.UserId);
        var uniqueEmail = $"{_factory.FakeUser.UserId}@example.com";
        await userGrain.RegisterAsync(uniqueEmail, "Test", "User");
    }

    [TearDown]
    public void TearDownClient()
    {
        _tenancyClient?.Dispose();
    }

    [OneTimeTearDown]
    public async Task TearDownFactory()
    {
        await _factory.DisposeAsync();
    }

    #region POST /tenants

    [Test]
    public async Task CreateTenant_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        var uniqueName = $"My New Tenant {Guid.CreateVersion7():N}";
        var request = new CreateTenantDto { Name = uniqueName, Description = "A test tenant" };

        // Act
        var result = await _tenancyClient.Client.CreateTenantAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(uniqueName);
        result.Description.ShouldBe("A test tenant");
        result.Name.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public async Task CreateTenant_ShouldReturnKebabCaseName()
    {
        // Arrange
        var uniqueId = Guid.CreateVersion7().ToString("N");
        var request = new CreateTenantDto { Name = $"Test Tenant With Spaces {uniqueId}", Description = "Testing kebab-case" };

        // Act
        var result = await _tenancyClient.Client.CreateTenantAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.SystemName.ShouldBe($"test-tenant-with-spaces-{uniqueId}");
    }

    [Test]
    public async Task CreateTenant_ShouldReturnConflict_WhenTenantAlreadyExists()
    {
        // Arrange - create the same tenant twice (using unique display name)
        var request = new CreateTenantDto { Name = $"Duplicate Tenant {Guid.CreateVersion7():N}", Description = "First creation" };

        // First creation
        await _tenancyClient.Client.CreateTenantAsync(request);

        // Act & Assert - second creation should fail with Conflict (409)
        var exception = await Should.ThrowAsync<ApiException>(() => _tenancyClient.Client.CreateTenantAsync(request));
        exception.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task CreateTenant_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        _factory.FakeUser.IsAuthenticated = false;
        var request = new CreateTenantDto { Name = "Unauthorized Tenant", Description = "Should fail" };

        // Act & Assert
        var exception = await Should.ThrowAsync<ApiException>(() => _tenancyClient.Client.CreateTenantAsync(request));
        exception.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task CreateTenant_ShouldReturnBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        var request = new CreateTenantDto { Name = string.Empty, Description = "Valid Description" };

        // Act & Assert
        var exception = await Should.ThrowAsync<ApiException>(() => _tenancyClient.Client.CreateTenantAsync(request));
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateTenant_ShouldReturnBadRequest_WhenNameIsTooLong()
    {
        // Arrange
        var request = new CreateTenantDto { Name = new string('a', 101), Description = "Valid Description" };

        // Act & Assert
        var exception = await Should.ThrowAsync<ApiException>(() => _tenancyClient.Client.CreateTenantAsync(request));
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateTenant_ShouldReturnBadRequest_WhenUserNotRegistered()
    {
        // Arrange
        // Use a new user ID that hasn't been registered in SetUp
        _factory.FakeUser.UserId = $"unregistered-user-{Guid.CreateVersion7():N}";
        var request = new CreateTenantDto { Name = "Unregistered User Tenant", Description = "Should fail" };

        // Act & Assert
        var exception = await Should.ThrowAsync<ApiException>(() => _tenancyClient.Client.CreateTenantAsync(request));
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        // We can't easily check the error message with Refit ApiException without parsing the content, 
        // but the status code confirms the behavior was intercepted.
    }

    #endregion

    #region GET /tenants

    [Test]
    public async Task GetTenant_ShouldReturnOk_WhenTenantExists()
    {
        // Arrange - first create the tenant
        var tenantName = $"existing-tenant-{Guid.CreateVersion7():N}";
        var request = new CreateTenantDto { Name = tenantName, Description = "Existing tenant" };
        var createdTenant = await _tenancyClient.Client.CreateTenantAsync(request);
        createdTenant.ShouldNotBeNull();

        // Create a new client scoped to the created tenant
        using var tenantClientWrapper = _factory.CreateTypedClient<ITenancyClient>(createdTenant.SystemName);

        // Act
        var result = await tenantClientWrapper.Client.GetTenantAsync();

        // Assert
        result.ShouldNotBeNull();
        result.SystemName.ShouldBe(createdTenant.SystemName);
        result.Name.ShouldBe(createdTenant.Name);
    }

    [Test]
    public async Task GetTenant_ShouldReturnBadRequest_WhenTenantCannotBeResolved()
    {
        // Arrange - use a non-existent tenant
        using var tenantClientWrapper = _factory.CreateTypedClient<ITenancyClient>("nonexistent-tenant");

        // Act & Assert
        // Strict tenant enforcement returns BadRequest when tenant cannot be resolved
        var exception = await Should.ThrowAsync<ApiException>(() => tenantClientWrapper.Client.GetTenantAsync());
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetTenant_ShouldReturnBadRequest_WhenNoTenantHeader()
    {
        // Arrange - don't set the x-tenant header

        // Act & Assert
        // Without tenant header, the multi-tenant resolution should fail
        var exception = await Should.ThrowAsync<ApiException>(() => _tenancyClient.Client.GetTenantAsync());
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetTenant_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        // 1. Create a tenant while authenticated
        var uniqueSuffix = Guid.CreateVersion7().ToString("N");
        var tenantName = $"auth-test-tenant-{uniqueSuffix}";
        var tenantId = $"auth-test-tenant-{uniqueSuffix}"; // Kebab case

        // Seed directly via grain to ensure existence
        using (var scope = _factory.Services.CreateScope())
        {
            var grainFactory = scope.ServiceProvider.GetRequiredService<IGrainFactory>();
            var tenantGrain = grainFactory.GetGrain<ITenantGrain>(tenantId);
            await tenantGrain.CreateAsync(new CreateTenantGrainCommand
            {
                DisplayName = tenantName,
                Description = "Tenant for auth test"
            });
        }

        // 2. De-authenticate
        _factory.FakeUser.IsAuthenticated = false;

        using var tenantClientWrapper = _factory.CreateTypedClient<ITenancyClient>(tenantId);

        // Act & Assert
        var exception = await Should.ThrowAsync<ApiException>(() => tenantClientWrapper.Client.GetTenantAsync());
        exception.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /tenants (List)

    [Test]
    public async Task GetTenantsList_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        _factory.FakeUser.IsAuthenticated = false;

        // Act & Assert
        var exception = await Should.ThrowAsync<ApiException>(() => _tenancyClient.Client.GetTenantsListAsync());
        exception.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetTenantsList_ShouldReturnEmptyList_WhenUserHasNoTenants()
    {
        // Arrange - user is registered but has no tenants

        // Act
        var result = await _tenancyClient.Client.GetTenantsListAsync();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetTenantsList_ShouldReturnSingleTenant_WhenUserHasOneTenant()
    {
        // Arrange
        var uniqueName = $"Single Tenant {Guid.CreateVersion7():N}";
        var request = new CreateTenantDto { Name = uniqueName, Description = "Single tenant test" };
        var createdTenant = await _tenancyClient.Client.CreateTenantAsync(request);

        // Act
        var result = await _tenancyClient.Client.GetTenantsListAsync();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem();
        result[0].SystemName.ShouldBe(createdTenant.SystemName);
        result[0].Name.ShouldBe(createdTenant.Name);
    }

    [Test]
    public async Task GetTenantsList_ShouldReturnMultipleTenants_WhenUserHasMultipleTenants()
    {
        // Arrange - create 3 tenants
        var uniqueSuffix = Guid.CreateVersion7().ToString("N");
        var tenant1 = await _tenancyClient.Client.CreateTenantAsync(new CreateTenantDto
        {
            Name = $"Tenant One {uniqueSuffix}",
            Description = "First tenant"
        });

        var tenant2 = await _tenancyClient.Client.CreateTenantAsync(new CreateTenantDto
        {
            Name = $"Tenant Two {uniqueSuffix}",
            Description = "Second tenant"
        });

        var tenant3 = await _tenancyClient.Client.CreateTenantAsync(new CreateTenantDto
        {
            Name = $"Tenant Three {uniqueSuffix}",
            Description = "Third tenant"
        });

        // Act
        var result = await _tenancyClient.Client.GetTenantsListAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.ShouldContain(t => t.SystemName == tenant1.SystemName);
        result.ShouldContain(t => t.SystemName == tenant2.SystemName);
        result.ShouldContain(t => t.SystemName == tenant3.SystemName);
    }

    [Test]
    public async Task GetTenantsList_ShouldReturnDifferentLists_ForDifferentUsers()
    {
        // Arrange - create tenants for user A
        var userAId = _factory.FakeUser.UserId;
        var userATenant = await _tenancyClient.Client.CreateTenantAsync(new CreateTenantDto
        {
            Name = $"User A Tenant {Guid.CreateVersion7():N}",
            Description = "Belongs to user A"
        });

        // Switch to user B
        _factory.FakeUser.UserId = $"test-user-b-{Guid.CreateVersion7():N}";
        _factory.FakeUser.TenantId = $"test-tenant-b-{Guid.CreateVersion7():N}";

        // Register user B
        using var scope = _factory.Services.CreateScope();
        var grainFactory = scope.ServiceProvider.GetRequiredService<IGrainFactory>();
        var userBGrain = grainFactory.GetGrain<IUserGrain>(_factory.FakeUser.UserId);
        await userBGrain.RegisterAsync($"{_factory.FakeUser.UserId}@example.com", "User", "B");

        // Create client for user B
        using var userBClient = _factory.CreateTypedClient<ITenancyClient>();

        // Create tenant for user B
        var userBTenant = await userBClient.Client.CreateTenantAsync(new CreateTenantDto
        {
            Name = $"User B Tenant {Guid.CreateVersion7():N}",
            Description = "Belongs to user B"
        });

        // Act - get tenants for both users
        // Get User B's list first (while still authenticated as User B)
        var userBResult = await userBClient.Client.GetTenantsListAsync();

        // Switch back to user A and get their list
        _factory.FakeUser.UserId = userAId;
        using var userAClient = _factory.CreateTypedClient<ITenancyClient>();
        var userAResult = await userAClient.Client.GetTenantsListAsync();

        // Assert - each user sees only their own tenants
        userAResult.ShouldNotBeNull();
        userAResult.ShouldHaveSingleItem();
        userAResult[0].SystemName.ShouldBe(userATenant.SystemName);

        userBResult.ShouldNotBeNull();
        userBResult.ShouldHaveSingleItem();
        userBResult[0].SystemName.ShouldBe(userBTenant.SystemName);

    }

    #endregion
}
