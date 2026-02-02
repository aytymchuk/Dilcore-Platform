using Microsoft.Extensions.Logging;

namespace Dilcore.Authentication.Orleans.Extensions;

/// <summary>
/// Incoming grain call filter that extracts user context from Orleans RequestContext
/// and makes it available within the grain method execution.
/// </summary>
public sealed class UserIncomingGrainCallFilter : IIncomingGrainCallFilter
{
    private readonly ILogger<UserIncomingGrainCallFilter> _logger;

    public UserIncomingGrainCallFilter(ILogger<UserIncomingGrainCallFilter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Intercepts incoming grain calls to extract and validate user context.
    /// </summary>
    public async Task Invoke(IIncomingGrainCallContext context)
    {
        var userContext = OrleansUserContextAccessor.GetUserContext();

        if (userContext is not null)
        {
            _logger.LogUserContextExtracted(
                context.Grain?.GetType().Name ?? "Unknown",
                context.InterfaceMethod?.Name ?? "Unknown",
                userContext.Id,
                userContext.Email);
        }
        else
        {
            _logger.LogUserContextNotFound(
                context.Grain?.GetType().Name ?? "Unknown",
                context.InterfaceMethod?.Name ?? "Unknown");
        }

        // Continue with the grain call
        await context.Invoke();
    }
}
