namespace Dilcore.Tenancy.Actors.Abstractions;

/// <summary>
/// Data transfer object for tenant information.
/// </summary>
/// <param name="Name">The human-readable display name.</param>
/// <param name="SystemName">The unique system name (lower kebab-case identifier).</param>
/// <param name="Description">Optional description of the tenant.</param>
/// <param name="CreatedAt">When the tenant was created.</param>
[GenerateSerializer]
public sealed record TenantDto(
    Guid Id,
    string Name,
    string SystemName,
    string? Description,
    string StorageIdentifier,
    DateTime CreatedAt);