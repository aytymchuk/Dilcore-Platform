using AutoMapper;
using Dilcore.MediatR.Abstractions;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.Results.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Domain;
using FluentResults;

namespace Dilcore.Tenancy.Core.Features.Get;

/// <summary>
/// Handles getting the current tenant via the TenantGrain.
/// Uses ITenantContext.Name as the grain key.
/// </summary>
public class GetTenantHandler : IQueryHandler<GetTenantQuery, Tenant>
{
    private readonly ITenantContext _tenantContext;
    private readonly IGrainFactory _grainFactory;
    private readonly IMapper _mapper;

    public GetTenantHandler(ITenantContext tenantContext, IGrainFactory grainFactory, IMapper mapper)
    {
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _grainFactory = grainFactory ?? throw new ArgumentNullException(nameof(grainFactory));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<Tenant>> Handle(GetTenantQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_tenantContext.Name))
        {
            return Result.Fail<Tenant>(new ValidationError("Tenant name is required"));
        }

        var grain = _grainFactory.GetGrain<ITenantGrain>(_tenantContext.Name);
        var result = await grain.GetAsync();

        if (result is null)
        {
            return Result.Fail<Tenant>(new NotFoundError("Tenant", _tenantContext.Name!));
        }

        return Result.Ok(_mapper.Map<Tenant>(result));
    }
}