using Dilcore.MediatR.Abstractions;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.Results.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using FluentResults;

namespace Dilcore.Tenancy.Core.Features.Get;

/// <summary>
/// Handles getting the current tenant via the TenantGrain.
/// Uses ITenantContext.Name as the grain key.
/// </summary>
public sealed class GetTenantHandler : IQueryHandler<GetTenantQuery, TenantDto>
{
    private readonly ITenantContext _tenantContext;
    private readonly IGrainFactory _grainFactory;

    public GetTenantHandler(ITenantContext tenantContext, IGrainFactory grainFactory)
    {
        _tenantContext = tenantContext;
        _grainFactory = grainFactory;
    }

    public async Task<Result<TenantDto>> Handle(GetTenantQuery request, CancellationToken cancellationToken)
    {
        if (_tenantContext.Name is null)
        {
            return Result.Fail<TenantDto>(new ValidationError("Tenant name is required"));
        }

        var grain = _grainFactory.GetGrain<ITenantGrain>(_tenantContext.Name);
        var result = await grain.GetAsync();

        if (result is null)
        {
            return Result.Fail<TenantDto>(new NotFoundError("Tenant", _tenantContext.Name!));
        }

        return Result.Ok(result);
    }
}