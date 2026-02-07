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

        var result = await _repository.StoreAsync(document, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToResult<Tenant>();
        }

        return Result.Ok(result.Value.ToDomain());
    }

    public async Task<Result<Tenant?>> GetBySystemNameAsync(string systemName, CancellationToken cancellationToken = default)
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

    public async Task<Result<IReadOnlyList<Tenant>>> GetBySystemNamesAsync(IEnumerable<string> systemNames, CancellationToken cancellationToken = default)
    {
        var systemNamesList = systemNames.ToList();

        if (systemNamesList.Count == 0)
        {
            return Result.Ok<IReadOnlyList<Tenant>>([]);
        }

        var filter = Builders<TenantDocument>.Filter.In(x => x.SystemName, systemNamesList);
        var result = await _repository.GetListAsync(filter, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToResult<IReadOnlyList<Tenant>>();
        }

        var documents = result.Value ?? [];

        return Result.Ok<IReadOnlyList<Tenant>>(documents.Select(d => d.ToDomain()).ToList());
    }
}
