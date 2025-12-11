using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace WebApi.Extensions;

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

        // Example: Extract Tenant ID from headers or claims
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
        {
            data.Attributes = (data.Attributes ?? Enumerable.Empty<KeyValuePair<string, object?>>())
                .Append(new KeyValuePair<string, object?>("tenant.id", tenantId.ToString()))
                .ToList();
        }

        // Example: Extract User ID from User Identity
        var userId = context.User?.Identity?.Name ?? "anonymous";
        data.Attributes = (data.Attributes ?? Enumerable.Empty<KeyValuePair<string, object?>>())
            .Append(new KeyValuePair<string, object?>("user.id", userId))
            .ToList();

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

    public override void OnStart(Activity data)
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

        base.OnStart(data);
    }
}
