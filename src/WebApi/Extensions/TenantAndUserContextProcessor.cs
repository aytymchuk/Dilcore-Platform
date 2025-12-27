using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Dilcore.WebApi.Extensions;

public class TenantAndUserContextProcessor : BaseProcessor<LogRecord>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantAndUserContextProcessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override void OnEnd(LogRecord data)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return;
        }

        var attributes = data.Attributes?.ToList() ?? new List<KeyValuePair<string, object?>>();

        // Remove existing tenant.id and user.id to avoid duplicates
        attributes.RemoveAll(kv => kv.Key == "tenant.id" || kv.Key == "user.id");

        // Example: Extract Tenant ID from headers or claims
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
        {
            attributes.Add(new KeyValuePair<string, object?>("tenant.id", tenantId.ToString()));
        }

        // Example: Extract User ID from User Identity
        var userId = context.User?.Identity?.Name ?? "anonymous";
        attributes.Add(new KeyValuePair<string, object?>("user.id", userId));

        data.Attributes = attributes;

        base.OnEnd(data);
    }
}

public class TenantAndUserActivityProcessor : BaseProcessor<Activity>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantAndUserActivityProcessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override void OnEnd(Activity data)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return;
        }

        // Example: Extract Tenant ID from headers or claims
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
        {
            data.SetTag("tenant.id", tenantId.ToString());
        }

        // Example: Extract User ID from User Identity
        var userId = context.User?.Identity?.Name ?? "anonymous";
        data.SetTag("user.id", userId);

        base.OnEnd(data);
    }
}
