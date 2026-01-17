using Dilcore.CorrelationId.Abstractions;
using Microsoft.Extensions.Logging;

namespace Dilcore.CorrelationId.Http.Extensions;

/// <summary>
/// Resolves correlation ID context lazily using registered providers.
/// Registered as a singleton for use in telemetry enrichment (OpenTelemetry) where
/// per-request scoped services are not available. The resolver safely accesses per-request
/// data by delegating to ICorrelationIdContextProvider implementations, which use IHttpContextAccessor
/// to retrieve the current request's HttpContext. This design allows the singleton resolver
/// to be injected into singleton telemetry processors while still resolving correlation ID data
/// correctly for each request.
/// </summary>
public sealed class CorrelationIdContextResolver : ICorrelationIdContextResolver
{
    private readonly IEnumerable<ICorrelationIdContextProvider> _providers;
    private readonly ILogger<CorrelationIdContextResolver> _logger;

    public CorrelationIdContextResolver(
        IEnumerable<ICorrelationIdContextProvider> providers,
        ILogger<CorrelationIdContextResolver> logger)
    {
        _providers = providers.OrderByDescending(p => p.Priority);
        _logger = logger;
    }

    public ICorrelationIdContext Resolve()
    {
        if (TryResolve(out var correlationIdContext))
        {
            return correlationIdContext!;
        }

        _logger.LogNoCorrelationIdResolved();
        return CorrelationIdContext.Empty;
    }

    public bool TryResolve(out ICorrelationIdContext? correlationIdContext)
    {
        foreach (var provider in _providers)
        {
            var context = provider.GetCorrelationIdContext();
            if (context != null && !string.IsNullOrWhiteSpace(context.CorrelationId))
            {
                _logger.LogCorrelationIdResolved(provider.GetType().Name, context.CorrelationId);
                correlationIdContext = context;
                return true;
            }
        }

        correlationIdContext = null;
        return false;
    }
}
