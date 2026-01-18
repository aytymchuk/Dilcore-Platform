using Dilcore.MultiTenant.Abstractions;

namespace Dilcore.MultiTenant.Orleans.Extensions;

/// <summary>
/// Provides tenant context from Orleans RequestContext.
/// This provider has a higher priority (200) than the HTTP provider (100),
/// so it will be checked first in Orleans grain contexts.
/// </summary>
public sealed class OrleansTenantContextProvider : ITenantContextProvider
{
    /// <summary>
    /// Priority of this provider. Higher values are checked first.
    /// Set to 200 to prioritize over HTTP-based providers (100).
    /// </summary>
    public int Priority => 200;

    /// <summary>
    /// Gets the tenant context from Orleans RequestContext.
    /// </summary>
    /// <returns>The tenant context if available in Orleans RequestContext, otherwise null.</returns>
    public ITenantContext? GetTenantContext()
    {
        return OrleansTenantContextAccessor.GetTenantContext();
    }
}
