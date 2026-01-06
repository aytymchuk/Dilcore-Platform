using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.Abstractions.Exceptions;
using Microsoft.Extensions.Logging;

namespace Dilcore.MultiTenant.Http.Extensions;

/// <summary>
/// Resolves tenant context lazily using registered providers.
/// Scoped lifetime ensures resolution happens once per request.
/// </summary>
public sealed class TenantContextResolver : ITenantContextResolver
{
    private readonly IEnumerable<ITenantContextProvider> _providers;
    private readonly ILogger<TenantContextResolver> _logger;

    public TenantContextResolver(
        IEnumerable<ITenantContextProvider> providers,
        ILogger<TenantContextResolver> logger)
    {
        _providers = providers.OrderByDescending(p => p.Priority);
        _logger = logger;
    }

    public ITenantContext Resolve()
    {
        if (TryResolve(out var tenantContext))
        {
            return tenantContext!;
        }

        _logger.LogNoTenantResolved();
        throw new TenantNotResolvedException("No tenant could be resolved from the current request.");
    }

    public bool TryResolve(out ITenantContext? tenantContext)
    {
        foreach (var provider in _providers)
        {
            var context = provider.GetTenantContext();
            if (context != null)
            {
                _logger.LogTenantResolved(provider.GetType().Name, context.Name ?? "Unknown");
                tenantContext = context;
                return true;
            }
        }

        tenantContext = null;
        return false;
    }
}