namespace Dilcore.WebApi.Infrastructure.MultiTenant;

/// <summary>
/// Resolves tenant context by trying registered providers in priority order.
/// </summary>
public interface ITenantContextResolver
{
    /// <summary>
    /// Resolves the current tenant context.
    /// Returns TenantContext.Empty if no tenant can be resolved.
    /// </summary>
    ITenantContext Resolve();
}