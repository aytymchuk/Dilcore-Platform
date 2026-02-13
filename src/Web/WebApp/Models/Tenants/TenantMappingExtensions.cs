using Dilcore.Tenancy.Contracts.Tenants;

namespace Dilcore.WebApp.Models.Tenants;

public static class TenantMappingExtensions
{
    public static Tenant ToModel(this TenantDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Tenant
        {
            Id = dto.Id,
            Name = dto.Name,
            SystemName = dto.SystemName,
            Description = dto.Description,
            StorageIdentifier = dto.StorageIdentifier,
            CreatedAt = dto.CreatedAt
        };
    }

    public static TenantDto ToContract(this Tenant entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new TenantDto
        {
            Id = entity.Id,
            Name = entity.Name,
            SystemName = entity.SystemName,
            StorageIdentifier = entity.StorageIdentifier,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt
        };
    }
}
