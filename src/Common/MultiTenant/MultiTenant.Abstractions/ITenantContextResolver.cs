namespace Dilcore.MultiTenant.Abstractions;

/// <summary>
/// Resolves tenant context by trying registered providers in priority order.
/// </summary>
public interface ITenantContextResolver
{
    /// <summary>
    /// Resolves the current tenant context.
    /// </summary>
    /// <returns>The resolved tenant context.</returns>
    /// <exception cref="Exceptions.TenantNotResolvedException">Thrown when tenant context cannot be resolved.</exception>
    ITenantContext Resolve();

    /// <summary>
    /// Attempts to resolve the tenant context.
    /// </summary>
    /// <param name="tenantContext">The resolved tenant context, or null if resolution failed.</param>
    /// <returns>True if tenant context was successfully resolved; otherwise, false.</returns>
    bool TryResolve(out ITenantContext? tenantContext);
}