namespace Dilcore.Tenancy.Actors.Abstractions;

/// <summary>
/// Data transfer object for tenant information.
/// </summary>
/// <param name="Name">The system name (lower kebab-case, unique identifier).</param>
/// <param name="DisplayName">The human-readable display name.</param>
/// <param name="Description">Optional description of the tenant.</param>
/// <param name="CreatedAt">When the tenant was created.</param>
[GenerateSerializer]
public sealed record TenantDto(
    string Name,
    string DisplayName,
    string Description,
    DateTime CreatedAt);
