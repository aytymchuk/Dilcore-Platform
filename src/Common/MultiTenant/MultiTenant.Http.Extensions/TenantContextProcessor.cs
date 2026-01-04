using Dilcore.MultiTenant.Abstractions;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Dilcore.MultiTenant.Http.Extensions;

/// <summary>
/// Enriches OpenTelemetry logs with tenant context information.
/// </summary>
public sealed class TenantContextProcessor : BaseProcessor<LogRecord>
{
    private readonly ITenantContextResolver _tenantContextResolver;

    public TenantContextProcessor(ITenantContextResolver tenantContextResolver)
    {
        _tenantContextResolver = tenantContextResolver;
    }

    public override void OnEnd(LogRecord data)
    {
        var attributes = data.Attributes?.ToList() ?? new List<KeyValuePair<string, object?>>();

        // Remove existing tenant.id to avoid duplicates
        attributes.RemoveAll(kv => kv.Key == "tenant.id");

        // Resolve tenant context using the resolver directly
        ITenantContext? tenantContext = null;
        try
        {
            tenantContext = _tenantContextResolver.Resolve();
        }
        catch (TenantNotResolvedException)
        {
            // Ignore if tenant is not resolved for logging
        }

        var tenantId = tenantContext?.Name;

        if (!string.IsNullOrEmpty(tenantId))
        {
            attributes.Add(new KeyValuePair<string, object?>("tenant.id", tenantId));
        }

        data.Attributes = attributes;

        base.OnEnd(data);
    }
}