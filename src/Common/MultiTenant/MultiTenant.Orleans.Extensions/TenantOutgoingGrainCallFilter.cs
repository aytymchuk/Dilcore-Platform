using Microsoft.Extensions.Logging;
using Orleans;

namespace Dilcore.MultiTenant.Orleans.Extensions;

/// <summary>
/// Outgoing grain call filter that propagates tenant context from the current execution context
/// to Orleans RequestContext, ensuring tenant information flows across grain calls.
/// </summary>
public sealed class TenantOutgoingGrainCallFilter : IOutgoingGrainCallFilter
{
    private readonly ITenantContextResolver _tenantContextResolver;
    private readonly ILogger<TenantOutgoingGrainCallFilter> _logger;

    public TenantOutgoingGrainCallFilter(
        ITenantContextResolver tenantContextResolver,
        ILogger<TenantOutgoingGrainCallFilter> logger)
    {
        _tenantContextResolver = tenantContextResolver;
        _logger = logger;
    }

    /// <summary>
    /// Intercepts outgoing grain calls to propagate tenant context.
    /// </summary>
    public async Task Invoke(IOutgoingGrainCallContext context)
    {
        // Try to resolve tenant context from the current execution context
        if (_tenantContextResolver.TryResolve(out var tenantContext) && tenantContext is not null)
        {
            // Set tenant context in Orleans RequestContext for propagation
            OrleansTenantContextAccessor.SetTenantContext(tenantContext);

            _logger.LogTenantContextPropagated(
                context.Grain?.GetType().Name ?? "Unknown",
                context.InterfaceMethod?.Name ?? "Unknown",
                tenantContext.Name ?? "null");
        }
        else
        {
            _logger.LogTenantContextNotPropagated(
                context.Grain?.GetType().Name ?? "Unknown",
                context.InterfaceMethod?.Name ?? "Unknown");
        }

        // Continue with the grain call
        await context.Invoke();
    }
}
