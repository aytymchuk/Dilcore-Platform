using Dilcore.MediatR.Abstractions;
using Dilcore.Tenancy.Domain;

namespace Dilcore.Tenancy.Core.Features.GetList;

/// <summary>
/// Query to get the list of tenants the current user has access to.
/// GET /tenants - Returns all tenants from the user's tenant list.
/// </summary>
public record GetTenantsListQuery : IQuery<IReadOnlyList<Tenant>>;
