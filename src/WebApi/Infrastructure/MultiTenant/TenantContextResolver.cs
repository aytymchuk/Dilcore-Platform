namespace Dilcore.WebApi.Infrastructure.MultiTenant;

/// <summary>
/// Resolves tenant context lazily using registered providers.
/// Scoped lifetime ensures resolution happens once per request.
/// </summary>
internal sealed class TenantContextResolver : ITenantContextResolver
{
    private readonly Lazy<ITenantContext> _lazyContext;

    public TenantContextResolver(
        IEnumerable<ITenantContextProvider> providers,
        ILogger<TenantContextResolver> logger)
    {
        _lazyContext = new Lazy<ITenantContext>(() =>
        {
            foreach (var provider in providers.OrderByDescending(p => p.Priority))
            {
                var context = provider.GetTenantContext();
                if (context != null)
                {
                    logger.LogDebug("Tenant resolved by {Provider}: {TenantName}",
                        provider.GetType().Name, context.Name);
                    return context;
                }
            }

            logger.LogDebug("No tenant resolved by any provider");
            return TenantContext.Empty;
        }, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public ITenantContext Resolve() => _lazyContext.Value;
}