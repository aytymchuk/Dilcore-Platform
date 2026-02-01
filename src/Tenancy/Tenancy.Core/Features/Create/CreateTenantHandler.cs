using Dilcore.MediatR.Abstractions;
using Dilcore.Results.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Domain;
using FluentResults;

namespace Dilcore.Tenancy.Core.Features.Create;

/// <summary>
/// Handles tenant creation by generating a kebab-case name and invoking TenantGrain.
/// </summary>
public sealed class CreateTenantHandler(IGrainFactory grainFactory)
    : ICommandHandler<CreateTenantCommand, TenantDto>
{
    private readonly IGrainFactory _grainFactory = grainFactory ?? throw new ArgumentNullException(nameof(grainFactory));

    public async Task<Result<TenantDto>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // 1. Generate SystemName
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Fail<TenantDto>(new ValidationError("Name is required"));
        }

        var systemName = Tenant.ToKebabCase(request.Name);

        var grain = _grainFactory.GetGrain<ITenantGrain>(systemName);

        var existingTenant = await grain.GetAsync();

        if (existingTenant is { IsCreated: true })
        {
            return Result.Fail<TenantDto>(new ConflictError("Tenant already exists"));
        }

        // 2. Create Tenant Grain
        // The grain handles uniqueness check atomically inside CreateAsync.
        var command = new CreateTenantGrainCommand
        {
            DisplayName = request.Name,
            Description = request.Description
        };

        var result = await grain.CreateAsync(command);

        if (!result.IsSuccess)
        {
            return Result.Fail(new ConflictError(result?.ErrorMessage ?? "Failed to create tenant"));
        }

        if (result.Tenant is null || !result.Tenant.IsCreated)
        {
            return Result.Fail<TenantDto>("Tenant creation failed");
        }

        return Result.Ok(result.Tenant);
    }
}
