using AutoMapper;
using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.MediatR.Abstractions;
using Dilcore.Results.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Domain;
using FluentResults;

namespace Dilcore.Tenancy.Core.Features.Create;

/// <summary>
/// Handles tenant creation by generating a kebab-case name and invoking TenantGrain.
/// </summary>
public class CreateTenantHandler : ICommandHandler<CreateTenantCommand, Tenant>
{
    private readonly IGrainFactory _grainFactory;
    private readonly IMapper _mapper;

    public CreateTenantHandler(IGrainFactory grainFactory, IMapper mapper)
    {
        _grainFactory = grainFactory ?? throw new ArgumentNullException(nameof(grainFactory));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<Tenant>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // 1. Generate SystemName
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Fail<Tenant>(new ValidationError("Name is required"));
        }

        var systemName = Tenant.ToKebabCase(request.Name);
        if (string.IsNullOrWhiteSpace(systemName))
        {
            return Result.Fail<Tenant>(new ValidationError("Tenant name is invalid after normalization"));
        }

        // 2. Validate Tenant Uniqueness
        var grain = _grainFactory.GetGrain<ITenantGrain>(systemName);

        var existingTenant = await grain.GetAsync();

        if (existingTenant is { IsCreated: true })
        {
            return Result.Fail<Tenant>(new ConflictError("Tenant already exists"));
        }

        // 3. Create Tenant Grain
        // The grain handles uniqueness check atomically inside CreateAsync.
        var command = new CreateTenantGrainCommand
        {
            DisplayName = request.Name,
            Description = request.Description
        };

        var result = await grain.CreateAsync(command);

        if (!result.IsSuccess)
        {
            return Result.Fail<Tenant>(new ConflictError(result?.ErrorMessage ?? "Failed to create tenant"));
        }

        if (result.Tenant is null || !result.Tenant.IsCreated)
        {
            return Result.Fail<Tenant>("Tenant creation failed");
        }

        return Result.Ok(_mapper.Map<Tenant>(result.Tenant));
    }
}