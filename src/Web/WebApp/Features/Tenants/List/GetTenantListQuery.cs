using Dilcore.WebApp.Models.Tenants;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApi.Client.Extensions;
using FluentResults;
using Dilcore.MediatR.Abstractions;

namespace Dilcore.WebApp.Features.Tenants.List;

/// <summary>
/// Query to retrieve the list of tenants.
/// </summary>
public record GetTenantListQuery : IQuery<List<Tenant>>;

/// <summary>
/// Handler for GetTenantListQuery.
/// </summary>
public class GetTenantListQueryHandler : IQueryHandler<GetTenantListQuery, List<Tenant>>
{
    private readonly ITenancyClient _tenancyClient;

    public GetTenantListQueryHandler(ITenancyClient tenancyClient)
    {
        _tenancyClient = tenancyClient;
    }

    public async Task<Result<List<Tenant>>> Handle(GetTenantListQuery request, CancellationToken cancellationToken)
    {
        var result = await _tenancyClient.SafeGetTenantsListAsync(cancellationToken);

        return result.Map(tenants => tenants.Select(t => t.ToModel()).ToList());
    }
}
