namespace Dilcore.Identity.Actors.Abstractions;

/// <summary>
/// Represents a user's access to a specific tenant.
/// </summary>
[GenerateSerializer]
public sealed record TenantAccess
{
    [Id(0)]
    public required string TenantId { get; init; }

    [Id(1)]
    public HashSet<string> Roles { get; init; } = [];
}
