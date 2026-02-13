using Dilcore.MediatR.Abstractions;
using Dilcore.Tenancy.Domain;

namespace Dilcore.Tenancy.Core.Features.Create;

/// <summary>
/// Command to create a new tenant.
/// POST /tenants - NOT tenant-specific (no x-tenant header required).
/// </summary>
/// <param name="Name">The human-readable display name.</param>
/// <param name="Description">Optional description of the tenant.</param>
public record CreateTenantCommand(string Name, string? Description) : ICommand<Tenant>
{
    public string SystemName { get; init; } = string.Empty;
}
