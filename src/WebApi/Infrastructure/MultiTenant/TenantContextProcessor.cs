using Dilcore.WebApi.Infrastructure.MultiTenant;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Dilcore.WebApi.Infrastructure.MultiTenant;

/// <summary>
/// Enriches OpenTelemetry logs with tenant context information.
/// </summary>
public class TenantContextProcessor : BaseProcessor<LogRecord>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContextProcessor(IHttpContextAccessor httpContextAccessor)
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

        // Remove existing tenant.id to avoid duplicates
        attributes.RemoveAll(kv => kv.Key == "tenant.id");

        // Extract Tenant ID - try ITenantContext first, fallback to header for compatibility
        string? tenantId = null;

        // Try to get from ITenantContext if RequestServices is available
        if (context.RequestServices != null)
        {
            var tenantContext = context.RequestServices.GetService<ITenantContext>();
            tenantId = tenantContext?.Name;
        }

        // Fallback to X-Tenant-ID header if not found via ITenantContext (for backward compatibility/testing)
        if (string.IsNullOrEmpty(tenantId) && context.Request.Headers.TryGetValue("X-Tenant-ID", out var headerValue))
        {
            tenantId = headerValue.ToString();
        }

        if (!string.IsNullOrEmpty(tenantId))
        {
            attributes.Add(new KeyValuePair<string, object?>("tenant.id", tenantId));
        }

        data.Attributes = attributes;

        base.OnEnd(data);
    }
}
