namespace Dilcore.Tenancy.Contracts.Tenants.Create;

/// <summary>
/// Data transfer object for creating a new tenant.
/// </summary>
public class CreateTenantDto
{
    /// <summary>
    /// Gets or sets the human-readable display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description of the tenant.
    /// </summary>
    public string? Description { get; set; }
}
