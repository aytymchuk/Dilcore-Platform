using Dilcore.MediatR.Abstractions;
using Dilcore.WebApp.Models.Tenants;

namespace Dilcore.WebApp.Features.Tenants.List;

/// <summary>
/// Query to retrieve the list of tenants.
/// </summary>
public record GetTenantListQuery : IQuery<List<Tenant>>;
