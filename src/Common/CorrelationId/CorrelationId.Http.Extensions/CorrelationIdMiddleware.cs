using Dilcore.CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dilcore.CorrelationId.Http.Extensions;

/// <summary>
/// Middleware that extracts or generates correlation ID for request tracking.
/// </summary>
public class CorrelationIdMiddleware : IMiddleware
{
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(ILogger<CorrelationIdMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string correlationId;

        // Try to get correlation ID from header
        if (context.Request.Headers.TryGetValue(CorrelationIdConstants.HeaderName, out var headerValue)
            && !string.IsNullOrWhiteSpace(headerValue.ToString()))
        {
            correlationId = headerValue.ToString()!;
            _logger.LogCorrelationIdExtracted(correlationId);
        }
        else
        {
            // Generate new correlation ID if not provided
            correlationId = Guid.NewGuid().ToString();
            _logger.LogCorrelationIdGenerated(correlationId);
        }

        // Store correlation ID in HttpContext.Items for access by provider
        context.Items[CorrelationIdConstants.HeaderName] = correlationId;

        // Add correlation ID to Activity baggage for Orleans/messaging propagation
        System.Diagnostics.Activity.Current?.SetBaggage(CorrelationIdConstants.HeaderName, correlationId);

        // Add correlation ID to response headers for client tracking
        context.Response.Headers.Append(CorrelationIdConstants.HeaderName, correlationId);

        await next(context);
    }
}
