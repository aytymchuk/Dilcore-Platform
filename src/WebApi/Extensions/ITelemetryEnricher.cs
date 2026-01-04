using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Dilcore.WebApi.Extensions;

public interface ITelemetryEnricher
{
    void Enrich(Activity activity, HttpRequest request);
}
