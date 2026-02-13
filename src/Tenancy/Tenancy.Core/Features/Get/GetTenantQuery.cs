using Dilcore.MediatR.Abstractions;
using Dilcore.Tenancy.Domain;

namespace Dilcore.Tenancy.Core.Features.Get;

/// <summary>
/// Query to get the current tenant details.
/// GET /tenants - Uses ITenantContext.Name as the grain key.
/// </summary>
public record GetTenantQuery : IQuery<Tenant>;
