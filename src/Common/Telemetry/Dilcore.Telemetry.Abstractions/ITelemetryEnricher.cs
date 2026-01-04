using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Dilcore.Telemetry.Abstractions;

public interface ITelemetryEnricher
{
    void Enrich(Activity activity, HttpRequest request);
}
