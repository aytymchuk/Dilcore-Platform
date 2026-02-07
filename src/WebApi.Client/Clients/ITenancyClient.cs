using Dilcore.Tenancy.Contracts.Tenants;
using Dilcore.Tenancy.Contracts.Tenants.Create;
using Refit;

namespace Dilcore.WebApi.Client.Clients;

/// <summary>
/// Refit client interface for Tenancy module endpoints.
/// </summary>
public interface ITenancyClient
{
    /// <summary>
    /// Creates a new tenant in the system.
    /// </summary>
    /// <param name="request">Tenant creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created tenant details.</returns>
    [Post("/tenants")]
    Task<TenantDto> CreateTenantAsync([Body] CreateTenantDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of tenants the authenticated user has access to.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of tenants the user has access to.</returns>
    [Get("/tenants")]
    Task<IReadOnlyList<TenantDto>> GetTenantsListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current tenant based on the x-tenant header.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current tenant details.</returns>
    [Get("/tenants/current")]
    Task<TenantDto> GetTenantAsync(CancellationToken cancellationToken = default);
}
