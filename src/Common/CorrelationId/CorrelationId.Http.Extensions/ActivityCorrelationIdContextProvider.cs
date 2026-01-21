using System.Diagnostics;
using Dilcore.CorrelationId.Abstractions;

namespace Dilcore.CorrelationId.Http.Extensions;

/// <summary>
/// Resolves correlation ID context from Activity.Current (for Orleans grains and background operations).
/// This provider works in scenarios where there's no HTTP context, such as:
/// - Orleans grain activations
/// - Background tasks
/// - Message handlers (when messaging is implemented)
/// </summary>
public sealed class ActivityCorrelationIdContextProvider : ICorrelationIdContextProvider
{
    // Lower priority than HTTP provider (checked second)
    public int Priority => 50;

    public ICorrelationIdContext? GetCorrelationIdContext()
    {
        var activity = Activity.Current;

        if (activity == null)
            return null;

        // Check if correlation ID was set as a tag/baggage
        var correlationId = activity.GetBaggageItem(CorrelationIdConstants.HeaderName)
            ?? activity.GetTagItem(CorrelationIdConstants.TelemetryTagName)?.ToString();

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            return new CorrelationIdContext(correlationId);
        }

        return null;
    }
}
