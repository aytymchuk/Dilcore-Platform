namespace Dilcore.Tenancy.Contracts.Tenants;

/// <summary>
/// Data transfer object for tenant information.
/// </summary>
public class TenantDto
{
    /// <summary>
    /// Gets or sets the human-readable display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique system name (lower kebab-case identifier).
    /// </summary>
    public string SystemName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description of the tenant.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets when the tenant was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
