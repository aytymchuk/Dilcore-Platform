using System.Diagnostics;
using Dilcore.Telemetry.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Dilcore.WebApi.Extensions;

public class UserTelemetryEnricher : ITelemetryEnricher
{
    public void Enrich(Activity activity, object request)
    {
        if (request is not HttpRequest httpRequest)
        {
            return;
        }

        var context = httpRequest.HttpContext;
        var userId = context.User?.Identity?.Name ?? "anonymous";
        activity.SetTag("user.id", userId);
    }
}