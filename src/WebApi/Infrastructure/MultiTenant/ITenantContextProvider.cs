namespace Dilcore.WebApi.Infrastructure.MultiTenant;

/// <summary>
/// Provider for resolving tenant context from different sources.
/// </summary>
public interface ITenantContextProvider
{
    /// <summary>
    /// Priority of this provider. Higher values = higher priority (checked first).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Attempts to resolve tenant context.
    /// Returns null if this provider cannot resolve the tenant.
    /// </summary>
    ITenantContext? GetTenantContext();
}