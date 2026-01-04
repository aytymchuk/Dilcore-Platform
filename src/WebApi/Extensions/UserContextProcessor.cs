using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Dilcore.WebApi.Extensions;

/// <summary>
/// Enriches OpenTelemetry logs with user context information.
/// </summary>
public class UserContextProcessor : BaseProcessor<LogRecord>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextProcessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override void OnEnd(LogRecord data)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            base.OnEnd(data);
            return;
        }

        var attributes = data.Attributes?.ToList() ?? new List<KeyValuePair<string, object?>>();

        // Remove existing user.id to avoid duplicates
        attributes.RemoveAll(kv => kv.Key == "user.id");

        // Extract User ID from User Identity
        var userId = context.User?.Identity?.Name ?? "anonymous";
        attributes.Add(new KeyValuePair<string, object?>("user.id", userId));

        data.Attributes = attributes;

        base.OnEnd(data);
    }
}
