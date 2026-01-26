using Microsoft.Extensions.Logging;

namespace Dilcore.MultiTenant.Orleans.Extensions;

/// <summary>
/// Incoming grain call filter that extracts tenant context from Orleans RequestContext
/// and makes it available within the grain method execution.
/// </summary>
public sealed class TenantIncomingGrainCallFilter : IIncomingGrainCallFilter
{
    private readonly ILogger<TenantIncomingGrainCallFilter> _logger;

    public TenantIncomingGrainCallFilter(ILogger<TenantIncomingGrainCallFilter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Intercepts incoming grain calls to extract and validate tenant context.
    /// </summary>
    public async Task Invoke(IIncomingGrainCallContext context)
    {
        var tenantContext = OrleansTenantContextAccessor.GetTenantContext();

        if (tenantContext is not null)
        {
            _logger.LogTenantContextExtracted(
                context.Grain?.GetType().Name ?? "Unknown",
                context.InterfaceMethod?.Name ?? "Unknown",
                tenantContext.Name,
                tenantContext.StorageIdentifier);
        }
        else
        {
            _logger.LogTenantContextNotFound(
                context.Grain?.GetType().Name ?? "Unknown",
                context.InterfaceMethod?.Name ?? "Unknown");
        }

        // Continue with the grain call
        await context.Invoke();
    }
}
