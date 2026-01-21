using Dilcore.CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Dilcore.CorrelationId.Http.Extensions;

/// <summary>
/// Resolves correlation ID context from HTTP context.
/// </summary>
public sealed class HttpCorrelationIdContextProvider(IHttpContextAccessor httpContextAccessor)
    : ICorrelationIdContextProvider
{
    public int Priority => 100;

    public ICorrelationIdContext? GetCorrelationIdContext()
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext == null)
            return null;

        // Retrieve correlation ID from HttpContext.Items (set by middleware)
        if (httpContext.Items.TryGetValue(CorrelationIdConstants.HeaderName, out var correlationId)
            && correlationId is string id && !string.IsNullOrWhiteSpace(id))
        {
            return new CorrelationIdContext(id);
        }

        return null;
    }
}
