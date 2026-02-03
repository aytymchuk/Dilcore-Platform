using Dilcore.Authentication.Abstractions;
using Microsoft.Extensions.Logging;

namespace Dilcore.Authentication.Orleans.Extensions;

/// <summary>
/// Outgoing grain call filter that propagates user context from the current execution context
/// to Orleans RequestContext, ensuring user information flows across grain calls.
/// </summary>
public sealed class UserOutgoingGrainCallFilter : IOutgoingGrainCallFilter
{
    private readonly IUserContextResolver _userContextResolver;
    private readonly ILogger<UserOutgoingGrainCallFilter> _logger;

    public UserOutgoingGrainCallFilter(
        IUserContextResolver userContextResolver,
        ILogger<UserOutgoingGrainCallFilter> logger)
    {
        _userContextResolver = userContextResolver;
        _logger = logger;
    }

    /// <summary>
    /// Intercepts outgoing grain calls to propagate user context.
    /// </summary>
    public async Task Invoke(IOutgoingGrainCallContext context)
    {
        // Try to resolve user context from the current execution context
        if (_userContextResolver.TryResolve(out var userContext) && userContext is not null)
        {
            // Set user context in Orleans RequestContext for propagation
            OrleansUserContextAccessor.SetUserContext(userContext);

            _logger.LogUserContextPropagated(
                context.Grain?.GetType().Name ?? "Unknown",
                context.InterfaceMethod?.Name ?? "Unknown",
                userContext.Id,
                userContext.Email);
        }
        else
        {
            // Clear any stale user context to prevent leakage between calls
            OrleansUserContextAccessor.SetUserContext(null);

            _logger.LogUserContextNotPropagated(
                context.Grain?.GetType().Name ?? "Unknown",
                context.InterfaceMethod?.Name ?? "Unknown");
        }

        // Continue with the grain call
        await context.Invoke();
    }
}
