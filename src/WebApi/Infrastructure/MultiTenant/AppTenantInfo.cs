using Finbuckle.MultiTenant.Abstractions;

namespace Dilcore.WebApi.Infrastructure.MultiTenant;

/// <summary>
/// Custom tenant info record that extends Finbuckle's TenantInfo with additional properties.
/// </summary>
public record AppTenantInfo(string Id, string Identifier, string? Name = null) : TenantInfo(Id, Identifier, Name)
{
    /// <summary>
    /// Custom property for storage identifier (e.g., database shard name, storage account).
    /// </summary>
    public string StorageIdentifier { get; set; } = string.Empty;
}