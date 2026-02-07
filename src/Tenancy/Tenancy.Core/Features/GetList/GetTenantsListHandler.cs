using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.MediatR.Abstractions;
using Dilcore.Tenancy.Core.Abstractions;
using Dilcore.Tenancy.Domain;
using FluentResults;

namespace Dilcore.Tenancy.Core.Features.GetList;

public class GetTenantsListHandler : IQueryHandler<GetTenantsListQuery, IReadOnlyList<Tenant>>
{
    private readonly IGrainFactory _grainFactory;
    private readonly IUserContext _userContext;
    private readonly ITenantRepository _tenantRepository;

    public GetTenantsListHandler(
        IGrainFactory grainFactory,
        IUserContext userContext,
        ITenantRepository tenantRepository)
    {
        _grainFactory = grainFactory;
        _userContext = userContext;
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<IReadOnlyList<Tenant>>> Handle(GetTenantsListQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.Id;
        var userGrain = _grainFactory.GetGrain<IUserGrain>(userId);
        var tenantAccessList = await userGrain.GetTenantsAsync();
        var tenantSystemNames = tenantAccessList.Select(ta => ta.TenantId).ToList();

        if (tenantSystemNames.Count == 0)
        {
            return Result.Ok<IReadOnlyList<Tenant>>(Array.Empty<Tenant>());
        }

        var tenantsResult = await _tenantRepository.GetBySystemNamesAsync(tenantSystemNames, cancellationToken);

        if (tenantsResult.IsFailed)
        {
            return tenantsResult.ToResult<IReadOnlyList<Tenant>>();
        }

        return Result.Ok<IReadOnlyList<Tenant>>(tenantsResult.Value);
    }
}
