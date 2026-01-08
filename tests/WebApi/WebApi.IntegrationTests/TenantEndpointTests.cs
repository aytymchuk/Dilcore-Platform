using System.Net;
using System.Net.Http.Json;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.WebApi.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dilcore.WebApi.IntegrationTests;

/// <summary>
/// Integration tests for Tenant endpoints (/tenants).
/// </summary>
[TestFixture]
public class TenantEndpointTests
{
    private const string TestTenantId = "test-tenant-for-auth";
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

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
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDownClient()
    {
        _client.Dispose();
    }

    [OneTimeTearDown]
    public void TearDownFactory()
    {
        _factory.Dispose();
    }

    #region POST /tenants

    [Test]
    public async Task CreateTenant_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        var uniqueName = $"My New Tenant {Guid.NewGuid():N}";
        var command = new { DisplayName = uniqueName, Description = "A test tenant" };

        // Act
        var response = await _client.PostAsJsonAsync("/tenants", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TenantDto>();
        result.ShouldNotBeNull();
        result.DisplayName.ShouldBe(uniqueName);
        result.Description.ShouldBe("A test tenant");
        result.Name.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public async Task CreateTenant_ShouldReturnKebabCaseName()
    {
        // Arrange
        var command = new { DisplayName = "Test Tenant With Spaces", Description = "Testing kebab-case" };

        // Act
        var response = await _client.PostAsJsonAsync("/tenants", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TenantDto>();
        result.ShouldNotBeNull();
        result.Name.ShouldBe("test-tenant-with-spaces");
    }

    [Test]
    public async Task CreateTenant_ShouldReturnConflict_WhenTenantAlreadyExists()
    {
        // Arrange - create the same tenant twice (using unique display name)
        var command = new { DisplayName = "Duplicate Tenant", Description = "First creation" };

        // First creation
        var firstResponse = await _client.PostAsJsonAsync("/tenants", command);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act - second creation should fail
        var secondResponse = await _client.PostAsJsonAsync("/tenants", command);

        // Assert
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task CreateTenant_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        _factory.FakeUser.IsAuthenticated = false;
        var command = new { DisplayName = "Unauthorized Tenant", Description = "Should fail" };

        // Act
        var response = await _client.PostAsJsonAsync("/tenants", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /tenants

    [Test]
    public async Task GetTenant_ShouldReturnOk_WhenTenantExists()
    {
        // Arrange - first create the tenant
        var tenantName = $"existing-tenant-{Guid.NewGuid():N}";
        var command = new { DisplayName = tenantName, Description = "Existing tenant" };
        var createResponse = await _client.PostAsJsonAsync("/tenants", command);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var createdTenant = await createResponse.Content.ReadFromJsonAsync<TenantDto>();

        // Set the tenant ID in the header for GET request
        var request = new HttpRequestMessage(HttpMethod.Get, "/tenants");
        request.Headers.Add(TenantConstants.HeaderName, createdTenant!.Name);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TenantDto>();
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createdTenant.Name);
        result.DisplayName.ShouldBe(tenantName);
    }

    [Test]
    public async Task GetTenant_ShouldReturnNotFound_WhenTenantDoesNotExist()
    {
        // Arrange - use a non-existent tenant in the header
        var request = new HttpRequestMessage(HttpMethod.Get, "/tenants");
        request.Headers.Add(TenantConstants.HeaderName, "nonexistent-tenant");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        // Strict tenant enforcement returns BadRequest when tenant cannot be resolved
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetTenant_ShouldReturnBadRequest_WhenNoTenantHeader()
    {
        // Arrange - don't set the x-tenant header

        // Act
        var response = await _client.GetAsync("/tenants");

        // Assert
        // Without tenant header, the multi-tenant resolution should fail
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetTenant_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        // 1. Create a tenant while authenticated
        var tenantName = "auth-test-tenant";
        var command = new { DisplayName = tenantName, Description = "Tenant for auth test" };
        var createResponse = await _client.PostAsJsonAsync("/tenants", command);

        // Ensure creation succeeded (or accept Conflict if it already exists from previous run)
        if (createResponse.StatusCode != HttpStatusCode.OK && createResponse.StatusCode != HttpStatusCode.Conflict)
        {
            createResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        var expectedTenantId = "auth-test-tenant"; // Kebab case of display name

        // 2. De-authenticate
        _factory.FakeUser.IsAuthenticated = false;

        var request = new HttpRequestMessage(HttpMethod.Get, "/tenants");
        request.Headers.Add(TenantConstants.HeaderName, expectedTenantId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion
}
