using Dilcore.MediatR.Abstractions;
using Dilcore.WebApp.Models.Tenants;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApi.Client.Extensions;
using FluentResults;

namespace Dilcore.WebApp.Features.Tenants.List;

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
