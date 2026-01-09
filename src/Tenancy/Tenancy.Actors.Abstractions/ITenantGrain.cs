

namespace Dilcore.Tenancy.Actors.Abstractions;

/// <summary>
/// Represents a tenant entity in the Orleans actor system.
/// Grain key is the tenant name from ITenantContext.Name.
/// </summary>
public interface ITenantGrain : IGrainWithStringKey
{
    /// <summary>
    /// Creates a new tenant with the provided information.
    /// The grain key becomes the system name (lower kebab-case).
    /// </summary>
    /// <param name="displayName">The human-readable display name.</param>
    /// <param name="description">Optional description of the tenant.</param>
    /// <returns>The created tenant details or a failure result.</returns>
    Task<TenantCreationResult> CreateAsync(string displayName, string description);

    /// <summary>
    /// Gets the tenant details.
    /// </summary>
    /// <returns>The tenant details, or null if not created.</returns>
    Task<TenantDto?> GetAsync();
}
