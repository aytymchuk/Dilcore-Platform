using Finbuckle.MultiTenant.Abstractions;

namespace Dilcore.MultiTenant.Abstractions;

/// <summary>
/// Custom tenant info record that extends Finbuckle's TenantInfo with additional properties.
/// </summary>
public record AppTenantInfo(string Id, string Identifier, string? Name, string StorageIdentifier) : ITenantInfo;