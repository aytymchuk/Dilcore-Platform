using Dilcore.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Abstractions;

namespace Dilcore.MultiTenant.Http.Extensions;

/// <summary>
/// Resolves tenant from HTTP context using Finbuckle's accessor.
/// </summary>
public sealed class HttpTenantContextProvider : ITenantContextProvider
{
    private readonly IMultiTenantContextAccessor<AppTenantInfo> _accessor;

    public HttpTenantContextProvider(IMultiTenantContextAccessor<AppTenantInfo> accessor)
    {
        _accessor = accessor;
    }

    public int Priority => 100;

    public ITenantContext? GetTenantContext()
    {
        var context = _accessor.MultiTenantContext;
        var tenantInfo = context?.TenantInfo;

        if (tenantInfo == null)
            return null;

        return new TenantContext(tenantInfo.Name, tenantInfo.StorageIdentifier);
    }
}