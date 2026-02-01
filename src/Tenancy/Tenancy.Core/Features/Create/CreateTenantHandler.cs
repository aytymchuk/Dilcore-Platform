using System.Text.RegularExpressions;
using Dilcore.MediatR.Abstractions;
using Dilcore.Results.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Core.Abstractions;
using FluentResults;

namespace Dilcore.Tenancy.Core.Features.Create;

/// <summary>
/// Handles tenant creation by generating a kebab-case name and invoking TenantGrain.
/// </summary>
public sealed partial class CreateTenantHandler : ICommandHandler<CreateTenantCommand, TenantDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IGrainFactory _grainFactory;

    public CreateTenantHandler(IGrainFactory grainFactory, ITenantRepository tenantRepository)
    {
        _grainFactory = grainFactory ?? throw new ArgumentNullException(nameof(grainFactory));
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
    }

    public async Task<Result<TenantDto>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // 1. Generate SystemName
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Fail<TenantDto>(new ValidationError("Name is required"));
        }

        request.SystemName = ToKebabCase(request.Name);

        var grain = _grainFactory.GetGrain<ITenantGrain>(request.SystemName);
        
        var tenant = await grain.GetAsync();
        
        // 2. Check Uniqueness
        if (tenant is not null)
        {
            return Result.Fail(new ConflictError($"Tenant with system name '{request.SystemName}' already exists."));
        }

        // 3. Create Tenant Grain
        var result = await grain.CreateAsync(request.Name, request.Description);

        if (!result.IsSuccess)
        {
            return Result.Fail(new ConflictError(result.ErrorMessage ?? "Failed to create tenant"));
        }

        if (result.Tenant is null)
        {
            return Result.Fail<TenantDto>("Tenant creation succeeded but returned null");
        }

        return Result.Ok(result.Tenant);
    }

    /// <summary>
    /// Converts a display name to lower kebab-case.
    /// Example: "My New Tenant" -> "my-new-tenant"
    /// </summary>
    private static string ToKebabCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Replace non-alphanumeric with spaces, then handle casing
        var normalized = KebabCaseRegex().Replace(input.Trim(), " ");
        var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join("-", parts).ToLowerInvariant();
    }

    [GeneratedRegex(@"[^a-zA-Z0-9\s]")]
    private static partial Regex KebabCaseRegex();
}
