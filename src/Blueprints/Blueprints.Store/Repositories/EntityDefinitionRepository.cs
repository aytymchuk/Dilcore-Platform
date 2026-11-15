using System.Text.RegularExpressions;
using AutoMapper;
using Dilcore.Blueprints.Core.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Blueprints.Store.Entities;
using Dilcore.MongoDB.Abstractions;
using Dilcore.MongoDB.Abstractions.Repositories;
using Dilcore.MongoDB.Abstractions.Results;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.Results.Abstractions;
using FluentResults;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Dilcore.Blueprints.Store.Repositories;

public sealed class EntityDefinitionRepository : IEntityDefinitionRepository
{
    private readonly IGenericRepository<EntityDefinitionDocument> _repository;
    private readonly IMapper _mapper;
    private readonly ITenantContextResolver _tenantContextResolver;
    private readonly IMongoDbCollectionFactory _collectionFactory;
    private readonly BlueprintsIndexInitializer _indexInitializer;

    public EntityDefinitionRepository(
        IGenericRepository<EntityDefinitionDocument> repository,
        IMapper mapper,
        ITenantContextResolver tenantContextResolver,
        IMongoDbCollectionFactory collectionFactory,
        BlueprintsIndexInitializer indexInitializer)
    {
        _repository = repository;
        _mapper = mapper;
        _tenantContextResolver = tenantContextResolver;
        _collectionFactory = collectionFactory;
        _indexInitializer = indexInitializer;
    }

    public async Task<Result<EntityDefinition>> StoreAsync(
        EntityDefinition entity, CancellationToken cancellationToken = default)
    {
        var indexes = await EnsureIndexesAsync(cancellationToken);
        if (indexes.IsFailed)
        {
            return indexes.ToResult<EntityDefinition>();
        }

        var document = _mapper.Map<EntityDefinitionDocument>(entity);
        var result = await _repository.StoreAsync(document, cancellationToken);

        if (result.HasError<DuplicateKeyError>())
        {
            return Result.Fail<EntityDefinition>(
                new ConflictError($"An entity definition with schema name '{entity.SchemaName}' already exists."));
        }

        if (result.IsFailed)
        {
            return result.ToResult<EntityDefinition>();
        }

        return Result.Ok(_mapper.Map<EntityDefinition>(result.Value));
    }

    public async Task<Result<EntityDefinition?>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var indexes = await EnsureIndexesAsync(cancellationToken);
        if (indexes.IsFailed)
        {
            return indexes.ToResult<EntityDefinition?>();
        }

        var filter = Builders<EntityDefinitionDocument>.Filter.Eq(x => x.Id, id);
        var result = await _repository.GetAsync(filter, cancellationToken);

        if (result.IsSuccess)
        {
            if (result.Value is null)
            {
                return Result.Ok<EntityDefinition?>(null);
            }

            return Result.Ok<EntityDefinition?>(_mapper.Map<EntityDefinition>(result.Value));
        }

        if (result.HasError<DocumentNotFoundError>())
        {
            return Result.Ok<EntityDefinition?>(null);
        }

        return result.ToResult<EntityDefinition?>();
    }

    public async Task<Result<IReadOnlyList<EntityDefinition>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var indexes = await EnsureIndexesAsync(cancellationToken);
        if (indexes.IsFailed)
        {
            return indexes.ToResult<IReadOnlyList<EntityDefinition>>();
        }

        var result = await _repository.GetListAsync(Builders<EntityDefinitionDocument>.Filter.Empty, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToResult<IReadOnlyList<EntityDefinition>>();
        }

        var documents = result.Value ?? [];
        return Result.Ok<IReadOnlyList<EntityDefinition>>(
            documents.Select(_mapper.Map<EntityDefinition>).ToList());
    }

    public async Task<Result> DeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var indexes = await EnsureIndexesAsync(cancellationToken);
        if (indexes.IsFailed)
        {
            return indexes;
        }

        var filter = Builders<EntityDefinitionDocument>.Filter.Eq(x => x.Id, id);
        var result = await _repository.DeleteAsync(filter, cancellationToken);
        return result.ToResult();
    }

    public async Task<Result<bool>> ExistsBySchemaNameAsync(
        string schemaName, CancellationToken cancellationToken = default)
    {
        var indexes = await EnsureIndexesAsync(cancellationToken);
        if (indexes.IsFailed)
        {
            return indexes.ToResult<bool>();
        }

        var filter = Builders<EntityDefinitionDocument>.Filter.Eq(x => x.SchemaName, schemaName);
        return await _repository.HasAnyAsync(filter, cancellationToken);
    }

    public async Task<Result<(IReadOnlyList<EntityDefinition> Items, long TotalCount)>> GetPagedAsync(
        int skip,
        int take,
        string? searchTerm = null,
        bool? isAbstract = null,
        IReadOnlyList<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfLessThan(take, 1);

        var indexes = await EnsureIndexesAsync(cancellationToken);
        if (indexes.IsFailed)
        {
            return indexes.ToResult<(IReadOnlyList<EntityDefinition>, long)>();
        }

        var filter = BuildFilter(searchTerm, isAbstract, tags);

        var countResult = await _repository.CountAsync(filter, cancellationToken);
        if (countResult.IsFailed)
        {
            return countResult.ToResult<(IReadOnlyList<EntityDefinition>, long)>();
        }

        var totalCount = countResult.Value;

        var items = new List<EntityDefinition>();
        var skipped = 0;
        await foreach (var doc in _repository.GetAsyncEnumerable(filter).WithCancellation(cancellationToken))
        {
            if (skipped < skip)
            {
                skipped++;
                continue;
            }

            items.Add(_mapper.Map<EntityDefinition>(doc));

            if (items.Count >= take)
            {
                break;
            }
        }

        return Result.Ok<(IReadOnlyList<EntityDefinition>, long)>((items, totalCount));
    }

    private async Task<Result> EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        if (!_tenantContextResolver.TryResolve(out var tenantContext) || tenantContext is null)
        {
            return Result.Ok();
        }

        if (string.IsNullOrWhiteSpace(tenantContext.StorageIdentifier))
        {
            return Result.Ok();
        }

        return await _indexInitializer.EnsureAsync(
            tenantContext.StorageIdentifier,
            _collectionFactory,
            cancellationToken);
    }

    private static FilterDefinition<EntityDefinitionDocument> BuildFilter(
        string? searchTerm, bool? isAbstract, IReadOnlyList<string>? tags)
    {
        var filters = new List<FilterDefinition<EntityDefinitionDocument>>();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            filters.Add(Builders<EntityDefinitionDocument>.Filter.Regex(
                x => x.DisplayName,
                new BsonRegularExpression(Regex.Escape(searchTerm), "i")));
        }

        if (isAbstract.HasValue)
        {
            filters.Add(Builders<EntityDefinitionDocument>.Filter.Eq(
                x => x.IsAbstract, isAbstract.Value));
        }

        if (tags is { Count: > 0 })
        {
            filters.Add(Builders<EntityDefinitionDocument>.Filter.AnyIn(
                x => x.Metadata.Tags, tags));
        }

        if (filters.Count == 0)
        {
            return Builders<EntityDefinitionDocument>.Filter.Empty;
        }

        return Builders<EntityDefinitionDocument>.Filter.And(filters);
    }
}
