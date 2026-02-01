using System.Text.RegularExpressions;
using Dilcore.MediatR.Abstractions;
using Dilcore.Results.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using FluentResults;

namespace Dilcore.Tenancy.Core.Features.Create;

/// <summary>
/// Handles tenant creation by generating a kebab-case name and invoking TenantGrain.
/// </summary>
public sealed partial class CreateTenantHandler(IGrainFactory grainFactory)
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

        var systemName = ToKebabCase(request.Name);

        var grain = _grainFactory.GetGrain<ITenantGrain>(systemName);

        var existingTenant = await grain.GetAsync();
        
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
