namespace Dilcore.WebApi.Infrastructure.MultiTenant;

/// <summary>
/// Immutable record representing resolved tenant context.
/// </summary>
internal sealed record TenantContext(string? Name, string? StorageIdentifier) : ITenantContext
{
    /// <summary>
    /// Represents a null/empty tenant context (no tenant resolved).
    /// </summary>
    public static readonly TenantContext Empty = new(null, null);
}
