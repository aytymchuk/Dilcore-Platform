using Dilcore.MediatR.Abstractions;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApi.Client.Extensions;
using Dilcore.WebApp.Models.Tenants;
using FluentResults;

namespace Dilcore.WebApp.Features.Tenants.Get;

/// <summary>
/// Handler for GetCurrentTenantQuery that retrieves the current tenant from the context.
/// </summary>
public class GetCurrentTenantQueryHandler : IQueryHandler<GetCurrentTenantQuery, Tenant>
{
    private readonly ITenancyClient _tenancyClient;

    public GetCurrentTenantQueryHandler(ITenancyClient tenancyClient)
    {
        _tenancyClient = tenancyClient;
    }

    public async Task<Result<Tenant>> Handle(GetCurrentTenantQuery request, CancellationToken cancellationToken)
    {
        var result = await _tenancyClient.SafeGetTenantAsync(cancellationToken);

        if (result.IsFailed)
        {
            return result.ToResult();
        }

        var tenantDto = result.Value;

        var tenant = tenantDto.ToModel();

        return Result.Ok(tenant);
    }
}
