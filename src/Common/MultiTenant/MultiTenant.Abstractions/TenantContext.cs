namespace Dilcore.MultiTenant.Abstractions;

/// <summary>
/// Immutable record representing resolved tenant context.
/// </summary>
public sealed record TenantContext(Guid Id, string? Name, string? StorageIdentifier) : ITenantContext
{
    /// <summary>
    /// Represents a null/empty tenant context (no tenant resolved).
    /// </summary>
    public static readonly TenantContext Empty = new(Guid.Empty, null, null);
}
