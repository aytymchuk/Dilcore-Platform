using Dilcore.Domain.Abstractions;

namespace Dilcore.Tenancy.Domain;

public sealed record Tenant : BaseDomain
{
    /// <summary>
    /// Prefix of the DB/Collection/Container name specified for this particular tenant.
    /// Cannot be changed after tenant creation.
    /// </summary>
    public required string StoragePrefix { get; init; }

    /// <summary>
    /// Unique system name (lower-kebab-case).
    /// </summary>
    public required string SystemName { get; init; }

    /// <summary>
    /// Display name of the tenant.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Explanation of the tenant (used for AI context).
    /// </summary>
    public string? Description { get; init; }
}
