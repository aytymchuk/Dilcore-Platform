namespace Dilcore.MultiTenant.Abstractions;

/// <summary>
/// Abstraction over Finbuckle's tenant context to minimize direct dependencies on the library.
/// Used for logging, messaging, and storage resolution.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Unique identifier of the tenant
    /// </summary>
    public Guid Id { get; }
    
    /// <summary>
    /// The name of the current tenant, or null if no tenant is resolved.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// The storage identifier for the tenant (e.g., database shard name), or null if no tenant is resolved.
    /// </summary>
    string? StorageIdentifier { get; }
}