using System.Diagnostics;

namespace Dilcore.Telemetry.Abstractions;

public interface ITelemetryEnricher
{
    void Enrich(Activity activity, object request);
}
