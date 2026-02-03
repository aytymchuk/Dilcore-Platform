using Dilcore.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;

namespace Dilcore.MultiTenant.Http.Extensions;

/// <summary>
/// Resolves tenant from HTTP context using Finbuckle's accessor.
/// </summary>
public sealed class HttpTenantContextProvider(IHttpContextAccessor httpContextAccessor) : ITenantContextProvider
{
    public int Priority => 100;

    public ITenantContext? GetTenantContext()
    {
        var context = httpContextAccessor.HttpContext?.GetMultiTenantContext<AppTenantInfo>();
        
        var tenantInfo = context?.TenantInfo;

        return tenantInfo == null
            ? null
            : new TenantContext(Guid.Parse(tenantInfo.Id), tenantInfo.Identifier, tenantInfo.StorageIdentifier);
    }
}