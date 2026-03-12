using Dilcore.MediatR.Abstractions;
using Dilcore.Tenancy.Contracts.Tenants;
using Dilcore.WebApp.Models.Tenants;

namespace Dilcore.WebApp.Features.Tenants.Create;

/// <summary>
/// Command to create a new tenant.
/// </summary>
/// <param name="Parameters">The parameters for creating the tenant.</param>
public record CreateTenantCommand(CreateTenantParameters Parameters) : ICommand<TenantDto>;
