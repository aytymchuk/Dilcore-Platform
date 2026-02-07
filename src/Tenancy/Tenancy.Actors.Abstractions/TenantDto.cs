namespace Dilcore.Tenancy.Actors.Abstractions;

/// <summary>
/// Data transfer object for tenant information.
/// </summary>
/// <param name="Id">The unique identifier (Guid) for the tenant.</param>
/// <param name="Name">The human-readable display name.</param>
/// <param name="SystemName">The unique system name (lower kebab-case identifier).</param>
/// <param name="Description">Optional description of the tenant.</param>
/// <param name="StorageIdentifier">The identifier used for tenant-specific storage or container/key.</param>
/// <param name="IsCreated">Whether the tenant has been created.</param>
/// <param name="CreatedAt">When the tenant was created.</param>
/// <param name="CreatedById">The user id of the person who created the tenant.</param>
[GenerateSerializer]
[Alias("Dilcore.Tenancy.Actors.Abstractions.TenantDto")]
public sealed record TenantDto(
    Guid Id,
    string Name,
    string SystemName,
    string? Description,
    string StorageIdentifier,
    bool IsCreated,
    DateTime CreatedAt,
    string CreatedById);