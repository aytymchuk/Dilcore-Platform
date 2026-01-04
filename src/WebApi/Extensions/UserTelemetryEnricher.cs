using System.Diagnostics;
using Dilcore.Telemetry.Abstractions;

namespace Dilcore.WebApi.Extensions;

public class UserTelemetryEnricher : ITelemetryEnricher
{
    public void Enrich(Activity activity, HttpRequest request)
    {
        var context = request.HttpContext;
        var userId = context.User.Identity?.Name ?? "anonymous";
        activity.SetTag("user.id", userId);
    }
}