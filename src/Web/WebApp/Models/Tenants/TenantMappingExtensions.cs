using Dilcore.Tenancy.Contracts.Tenants;

namespace Dilcore.WebApp.Models.Tenants;

public static class TenantMappingExtensions
{
    public static Tenant ToModel(this TenantDto dto)
    {
        return new Tenant
        {
            Id = dto.Id,
            Name = dto.Name,
            SystemName = dto.SystemName,
            Description = dto.Description,
            StoragePrefix = dto.StoragePrefix,
            CreatedAt = dto.CreatedAt
        };
    }
}
