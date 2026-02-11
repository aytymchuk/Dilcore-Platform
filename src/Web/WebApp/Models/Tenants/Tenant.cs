namespace Dilcore.WebApp.Models.Tenants;

/// <summary>
/// UI/view model for tenant information.
/// </summary>
public record Tenant
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the human-readable display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique system name (lower kebab-case identifier).
    /// </summary>
    public string SystemName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description of the tenant.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the storage prefix for the tenant.
    /// </summary>
    public string StoragePrefix { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets when the tenant was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
