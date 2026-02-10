using Dilcore.MediatR.Abstractions;
using Dilcore.WebApp.Models.Tenants;

namespace Dilcore.WebApp.Features.Tenants.Get;

/// <summary>
/// Query to get a tenant by its system name.
/// </summary>
public record GetTenantBySystemNameQuery(string SystemName) : IQuery<Tenant>;
