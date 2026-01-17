using Dilcore.CorrelationId.Abstractions;
using Dilcore.Telemetry.Abstractions;

namespace Dilcore.CorrelationId.Http.Extensions;

/// <summary>
/// Provides correlation ID as telemetry attributes for OpenTelemetry enrichment.
/// </summary>
public sealed class CorrelationIdAttributeProvider(ICorrelationIdContextResolver correlationIdContextResolver)
    : ITelemetryAttributeProvider
{
    public IEnumerable<KeyValuePair<string, object?>> GetAttributes()
    {
        if (correlationIdContextResolver.TryResolve(out var context)
            && !string.IsNullOrWhiteSpace(context?.CorrelationId))
        {
            yield return new KeyValuePair<string, object?>(
                CorrelationIdConstants.TelemetryTagName,
                context.CorrelationId);
        }
    }
}
