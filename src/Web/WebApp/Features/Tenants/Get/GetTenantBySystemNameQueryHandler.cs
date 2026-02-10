using Dilcore.MediatR.Abstractions;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApp.Models.Tenants;
using FluentResults;

namespace Dilcore.WebApp.Features.Tenants.Get;

/// <summary>
/// Handler for GetTenantBySystemNameQuery that retrieves tenant from the list and filters by system name.
/// </summary>
public class GetTenantBySystemNameQueryHandler : IQueryHandler<GetTenantBySystemNameQuery, Tenant>
{
    private readonly ITenancyClient _tenancyClient;

    public GetTenantBySystemNameQueryHandler(ITenancyClient tenancyClient)
    {
        _tenancyClient = tenancyClient;
    }

    public async Task<Result<Tenant>> Handle(GetTenantBySystemNameQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _tenancyClient.GetTenantsListAsync(cancellationToken);

        var tenantDto = tenants.FirstOrDefault(t => 
            string.Equals(t.SystemName, request.SystemName, StringComparison.OrdinalIgnoreCase));

        if (tenantDto == null)
        {
            return Result.Fail<Tenant>($"Tenant with system name '{request.SystemName}' not found.");
        }

        var tenant = new Tenant
        {
            Id = tenantDto.Id,
            Name = tenantDto.Name,
            SystemName = tenantDto.SystemName,
            Description = tenantDto.Description,
            StoragePrefix = tenantDto.StoragePrefix,
            CreatedAt = tenantDto.CreatedAt
        };

        return Result.Ok(tenant);
    }
}
