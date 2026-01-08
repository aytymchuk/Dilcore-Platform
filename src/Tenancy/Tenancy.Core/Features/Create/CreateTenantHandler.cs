using System.Text.RegularExpressions;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Results.Abstractions;
using FluentResults;
using MediatR;
using Dilcore.MediatR.Abstractions;

namespace Dilcore.Tenancy.Core.Features.Create;

/// <summary>
/// Handles tenant creation by generating a kebab-case name and invoking TenantGrain.
/// </summary>
public sealed partial class CreateTenantHandler : ICommandHandler<CreateTenantCommand, TenantDto>
{
    private readonly IGrainFactory _grainFactory;

    public CreateTenantHandler(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task<Result<TenantDto>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // Generate kebab-case tenant name from display name
        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return Result.Fail<TenantDto>(new ValidationError("DisplayName is required"));
        }

        var tenantName = ToKebabCase(request.DisplayName);

        var grain = _grainFactory.GetGrain<ITenantGrain>(tenantName);
        var result = await grain.CreateAsync(request.DisplayName, request.Description);

        if (result.IsSuccess)
        {
            return Result.Ok(result.Tenant!);
        }

        return Result.Fail(new ConflictError(result.ErrorMessage));
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
