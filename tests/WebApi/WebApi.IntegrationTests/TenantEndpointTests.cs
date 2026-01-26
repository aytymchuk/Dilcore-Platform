using System.Net;
using System.Net.Http.Json;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Contracts.Tenants;
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
    public void SetUpClient()
    {
        // Reset the fake user to defaults before each test
        _factory.FakeUser.UserId = $"test-user-{Guid.NewGuid():N}";
        _factory.FakeUser.TenantId = $"test-tenant-{Guid.NewGuid():N}";
        _factory.FakeUser.IsAuthenticated = true;
        _tenancyClient = _factory.CreateTypedClient<ITenancyClient>();
    }

    [TearDown]
    public void TearDownClient()
    {
        _tenancyClient.Dispose();
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
        var uniqueName = $"My New Tenant {Guid.NewGuid():N}";
        var request = new CreateTenantDto { DisplayName = uniqueName, Description = "A test tenant" };

        // Act
        var result = await _tenancyClient.Client.CreateTenantAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.DisplayName.ShouldBe(uniqueName);
        result.Description.ShouldBe("A test tenant");
        result.Name.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public async Task CreateTenant_ShouldReturnKebabCaseName()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N");
        var request = new CreateTenantDto { DisplayName = $"Test Tenant With Spaces {uniqueId}", Description = "Testing kebab-case" };

        // Act
        var result = await _tenancyClient.Client.CreateTenantAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe($"test-tenant-with-spaces-{uniqueId}");
    }

    [Test]
    public async Task CreateTenant_ShouldReturnConflict_WhenTenantAlreadyExists()
    {
        // Arrange - create the same tenant twice (using unique display name)
        var request = new CreateTenantDto { DisplayName = $"Duplicate Tenant {Guid.NewGuid():N}", Description = "First creation" };

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
        var request = new CreateTenantDto { DisplayName = "Unauthorized Tenant", Description = "Should fail" };

        // Act & Assert
        var exception = await Should.ThrowAsync<ApiException>(() => _tenancyClient.Client.CreateTenantAsync(request));
        exception.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /tenants

    [Test]
    public async Task GetTenant_ShouldReturnOk_WhenTenantExists()
    {
        // Arrange - first create the tenant
        var tenantName = $"existing-tenant-{Guid.NewGuid():N}";
        var request = new CreateTenantDto { DisplayName = tenantName, Description = "Existing tenant" };
        var createdTenant = await _tenancyClient.Client.CreateTenantAsync(request);
        createdTenant.ShouldNotBeNull();

        // Create a new client scoped to the created tenant
        using var tenantClientWrapper = _factory.CreateTypedClient<ITenancyClient>(createdTenant.Name);

        // Act
        var result = await tenantClientWrapper.Client.GetTenantAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createdTenant.Name);
        result.DisplayName.ShouldBe(tenantName);
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
        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var tenantName = $"auth-test-tenant-{uniqueSuffix}";
        var tenantId = $"auth-test-tenant-{uniqueSuffix}"; // Kebab case

        // Seed directly via grain to ensure existence
        using (var scope = _factory.Services.CreateScope())
        {
            var grainFactory = scope.ServiceProvider.GetRequiredService<IGrainFactory>();
            var tenantGrain = grainFactory.GetGrain<ITenantGrain>(tenantId);
            await tenantGrain.CreateAsync(tenantName, "Tenant for auth test");
        }

        // 2. De-authenticate
        _factory.FakeUser.IsAuthenticated = false;

        using var tenantClientWrapper = _factory.CreateTypedClient<ITenancyClient>(tenantId);

        // Act & Assert
        var exception = await Should.ThrowAsync<ApiException>(() => tenantClientWrapper.Client.GetTenantAsync());
        exception.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion
}
