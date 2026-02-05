using Dilcore.DocumentDb.Abstractions;
using Dilcore.Tenancy.Domain;

namespace Dilcore.Tenancy.Store.Entities;

public sealed class TenantDocument : IDocumentEntity
{
    public Guid Id { get; set; }
    public long ETag { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public required string StoragePrefix { get; set; }
    public required string SystemName { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public required string CreatedById { get; set; }

    public static TenantDocument FromDomain(Tenant tenant) => new()
    {
        Id = tenant.Id,
        StoragePrefix = tenant.StoragePrefix,
        SystemName = tenant.SystemName,
        Name = tenant.Name,
        Description = tenant.Description,
        CreatedById = tenant.CreatedById,
        CreatedAt = tenant.CreatedAt,
        UpdatedAt = tenant.UpdatedAt,
        ETag = tenant.ETag,
        IsDeleted = false
    };

    public Tenant ToDomain() => new()
    {
        Id = Id,
        StoragePrefix = StoragePrefix,
        SystemName = SystemName,
        Name = Name,
        Description = Description,
        CreatedById = string.IsNullOrWhiteSpace(CreatedById)
            ? throw new InvalidOperationException("Tenant must have a creator.")
            : CreatedById,
        CreatedAt = CreatedAt,
        UpdatedAt = UpdatedAt,
        ETag = ETag
    };
}
