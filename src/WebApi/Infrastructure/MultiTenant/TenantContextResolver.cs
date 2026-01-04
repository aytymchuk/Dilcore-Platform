namespace Dilcore.WebApi.Infrastructure.MultiTenant;

/// <summary>
/// Resolves tenant context lazily using registered providers.
/// Scoped lifetime ensures resolution happens once per request.
/// </summary>
internal sealed class TenantContextResolver : ITenantContextResolver
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
        foreach (var provider in _providers)
        {
            var context = provider.GetTenantContext();
            if (context != null)
            {
                _logger.LogDebug("Tenant resolved by {Provider}: {TenantName}",
                    provider.GetType().Name, context.Name);
                return context;
            }
        }

        _logger.LogDebug("No tenant resolved by any provider");
        return TenantContext.Empty;
    }
}