using Dilcore.MultiTenant.Abstractions;

namespace Dilcore.MultiTenant.Orleans.Extensions;

/// <summary>
/// Provides access to tenant context stored in Orleans RequestContext.
/// This allows tenant information to flow across grain calls.
/// </summary>
public static class OrleansTenantContextAccessor
{
    private const string TenantIdKey = "TenantContext.Id";
    private const string TenantNameKey = "TenantContext.Name";
    private const string TenantStorageIdentifierKey = "TenantContext.StorageIdentifier";

    /// <summary>
    /// Sets the tenant context in Orleans RequestContext.
    /// </summary>
    /// <param name="tenantContext">The tenant context to store.</param>
    public static void SetTenantContext(ITenantContext? tenantContext)
    {
        if (tenantContext is null)
        {
            RequestContext.Remove(TenantIdKey);
            RequestContext.Remove(TenantNameKey);
            RequestContext.Remove(TenantStorageIdentifierKey);
            return;
        }

        if (!tenantContext.Id.Equals(Guid.Empty))
        {
            RequestContext.Set(TenantIdKey, tenantContext.Id);
        }
        else
        {
            RequestContext.Remove(TenantIdKey);
        }

        if (tenantContext.Name is not null)
        {
            RequestContext.Set(TenantNameKey, tenantContext.Name);
        }
        else
        {
            RequestContext.Remove(TenantNameKey);
        }

        if (tenantContext.StorageIdentifier is not null)
        {
            RequestContext.Set(TenantStorageIdentifierKey, tenantContext.StorageIdentifier);
        }
        else
        {
            RequestContext.Remove(TenantStorageIdentifierKey);
        }
    }

    /// <summary>
    /// Gets the tenant context from Orleans RequestContext.
    /// </summary>
    /// <returns>The tenant context if available, otherwise null.</returns>
    public static ITenantContext? GetTenantContext()
    {
        var idObj = RequestContext.Get(TenantIdKey);
        if (idObj is null || !Guid.TryParse(idObj.ToString(), out var id))
        {
            return null;
        }

        var name = RequestContext.Get(TenantNameKey) as string;
        var storageIdentifier = RequestContext.Get(TenantStorageIdentifierKey) as string;

        return new TenantContext(id, name, storageIdentifier);
    }
}
