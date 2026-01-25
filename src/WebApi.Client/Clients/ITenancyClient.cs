using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Core.Features.Create;
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
    /// <param name="command">Tenant creation command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created tenant details.</returns>
    [Post("/tenants")]
    Task<TenantDto> CreateTenantAsync([Body] CreateTenantCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current tenant based on the x-tenant header.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current tenant details.</returns>
    [Get("/tenants")]
    Task<TenantDto> GetTenantAsync(CancellationToken cancellationToken = default);
}
