using Dilcore.Tenancy.Contracts.Tenants;
using Dilcore.Tenancy.Domain;

namespace Dilcore.Tenancy.Core.Extensions;

public static class TenantMappingExtensions
{
    public static TenantDto ToContract(this Tenant tenant)
    {
        if (tenant is null)
        {
            throw new ArgumentNullException(nameof(tenant));
        }

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            SystemName = tenant.SystemName,
            StoragePrefix = tenant.StoragePrefix,
            Description = tenant.Description,
            CreatedAt = tenant.CreatedAt
        };
    }
}
