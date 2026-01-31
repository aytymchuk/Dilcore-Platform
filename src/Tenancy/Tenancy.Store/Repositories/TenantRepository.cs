using Dilcore.DocumentDb.MongoDb.Repositories.Abstractions;
using Dilcore.Tenancy.Core.Abstractions;
using Dilcore.Tenancy.Domain;
using Dilcore.Tenancy.Store.Entities;
using FluentResults;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Dilcore.Tenancy.Store.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly IGenericRepository<TenantDocument> _repository;
    private readonly ILogger<TenantRepository> _logger;

    public TenantRepository(
        IGenericRepository<TenantDocument> repository,
        ILogger<TenantRepository> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Tenant>> StoreAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        var document = TenantDocument.FromDomain(tenant);
        
        try
        {
            var result = await _repository.StoreAsync(document, cancellationToken);
            
            if (result.IsFailed)
            {
                 return result.ToResult<Tenant>();
            }
            
            return Result.Ok(tenant);
        }
        catch (Exception ex)
        {
             _logger.LogStoreTenantFailed(ex, tenant.Id);
             return Result.Fail<Tenant>(new Error("Failed to store tenant").CausedBy(ex));
        }
    }

    public async Task<Result<Tenant?>> GetBySystemNameAsync(string systemName, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<TenantDocument>.Filter.Eq(x => x.SystemName, systemName);
            var result = await _repository.GetAsync(filter, cancellationToken);

            if (result.IsFailed)
            {
                 return result.ToResult<Tenant?>();
            }
            
            var document = result.Value;

            if (document is null)
            {
                return Result.Ok<Tenant?>(null);
            }
            
            return Result.Ok<Tenant?>(document.ToDomain());
        }
        catch (Exception ex)
        {
            _logger.LogGetTenantBySystemNameFailed(ex, systemName);
            return Result.Fail<Tenant?>(new Error("Failed to get tenant by system name").CausedBy(ex));
        }
    }
}
