using Dilcore.MultiTenant.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.WebApi.Extensions;
using Finbuckle.MultiTenant.Abstractions;

namespace Dilcore.WebApi.Infrastructure;

/// <summary>
/// A Finbuckle tenant store that uses Orleans grains to resolve tenant information.
/// This store queries the TenantGrain actor to get tenant details and maps them to AppTenantInfo.
/// </summary>
public sealed class OrleansTenantStore : IMultiTenantStore<AppTenantInfo>
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<OrleansTenantStore> _logger;

    public OrleansTenantStore(IGrainFactory grainFactory, ILogger<OrleansTenantStore> logger)
    {
        _grainFactory = grainFactory;
        _logger = logger;
    }

    /// <summary>
    /// Gets a tenant by its identifier (tenant name in kebab-case).
    /// </summary>
    public async Task<AppTenantInfo?> GetByIdentifierAsync(string identifier)
    {
        _logger.LogTenantStoreGetActorByIdentifier(identifier);
        if (string.IsNullOrWhiteSpace(identifier))
        {
            _logger.LogTenantStoreInvalidIdentifier();
            return null;
        }

        try
        {
            var grain = _grainFactory.GetGrain<ITenantGrain>(identifier);
            var tenantDto = await grain.GetAsync();

            if (tenantDto is null)
            {
                _logger.LogTenantStoreNotFound(identifier);
                return null;
            }

            var tenantInfo = MapToAppTenantInfo(tenantDto);
            _logger.LogTenantStoreResolved(identifier, tenantDto.DisplayName);
            return tenantInfo;
        }
        catch (Exception ex)
        {
            _logger.LogTenantStoreResolutionError(ex, identifier);
            throw;
        }
    }

    /// <summary>
    /// Gets a tenant by its ID.
    /// For Orleans grains, the ID and identifier are the same (tenant name).
    /// </summary>
    public Task<AppTenantInfo?> GetAsync(string id)
    {
        // In our implementation, ID and Identifier are the same (tenant name)
        return GetByIdentifierAsync(id);
    }

    /// <summary>
    /// Gets all tenants. Not implemented for Orleans grain store.
    /// </summary>
    public Task<IEnumerable<AppTenantInfo>> GetAllAsync()
    {
        // Orleans grains don't support listing all instances
        // Return empty collection
        _logger.LogTenantStoreGetAllNotSupported();
        return Task.FromResult(Enumerable.Empty<AppTenantInfo>());
    }

    /// <summary>
    /// Gets all tenants with pagination. Not implemented for Orleans grain store.
    /// </summary>
    public Task<IEnumerable<AppTenantInfo>> GetAllAsync(int offset, int count)
    {
        // Orleans grains don't support listing all instances
        _logger.LogTenantStoreGetAllNotSupported();
        return Task.FromResult(Enumerable.Empty<AppTenantInfo>());
    }

    /// <summary>
    /// Adds a new tenant. Not implemented - use TenantGrain.CreateAsync instead.
    /// </summary>
    public Task<bool> AddAsync(AppTenantInfo tenantInfo)
    {
        // Tenant creation should be done through TenantGrain.CreateAsync
        // This store is read-only for Finbuckle resolution
        _logger.LogTenantStoreModificationNotSupported("AddAsync");
        return Task.FromResult(false);
    }

    /// <summary>
    /// Updates a tenant. Not implemented - Orleans grains manage their own state.
    /// </summary>
    public Task<bool> UpdateAsync(AppTenantInfo tenantInfo)
    {
        _logger.LogTenantStoreModificationNotSupported("UpdateAsync");
        return Task.FromResult(false);
    }

    /// <summary>
    /// Removes a tenant. Not implemented - Orleans grains manage their own lifecycle.
    /// </summary>
    public Task<bool> RemoveAsync(string identifier)
    {
        _logger.LogTenantStoreModificationNotSupported("RemoveAsync");
        return Task.FromResult(false);
    }

    private static AppTenantInfo MapToAppTenantInfo(TenantDto dto)
    {
        return new AppTenantInfo(
            Id: dto.Name,           // Use tenant name as ID
            Identifier: dto.Name,   // Use tenant name as Identifier (what strategies match against)
            Name: dto.Name   // Use display name as the friendly Name
        )
        {
            // StorageIdentifier could be derived from tenant name for data isolation
            StorageIdentifier = dto.Name
        };
    }
}