using System.Text.RegularExpressions;
using AutoMapper;
using Dilcore.Blueprints.Core.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Blueprints.Store.Entities;
using Dilcore.DocumentDb.MongoDb.Repositories.Abstractions;
using Dilcore.Results.Abstractions;
using FluentResults;
using MongoDB.Driver;

namespace Dilcore.Blueprints.Store.Repositories;

public sealed class EntityDefinitionRepository : IEntityDefinitionRepository
{
    private static readonly FilterDefinition<EntityDefinitionDocument> NotDeleted =
        Builders<EntityDefinitionDocument>.Filter.Eq(x => x.IsDeleted, false);

    private readonly IGenericRepository<EntityDefinitionDocument> _repository;
    private readonly IMapper _mapper;

    public EntityDefinitionRepository(
        IGenericRepository<EntityDefinitionDocument> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<EntityDefinition>> StoreAsync(
        EntityDefinition entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = _mapper.Map<EntityDefinitionDocument>(entity);
            var result = await _repository.StoreAsync(document, cancellationToken);

            return result.IsFailed
                ? result.ToResult<EntityDefinition>()
                : Result.Ok(_mapper.Map<EntityDefinition>(result.Value));
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return Result.Fail<EntityDefinition>(
                new ConflictError($"An entity definition with schema name '{entity.SchemaName}' already exists."));
        }
    }

    public async Task<Result<EntityDefinition?>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<EntityDefinitionDocument>.Filter.Eq(x => x.Id, id)
                     & NotDeleted;
        var result = await _repository.GetAsync(filter, cancellationToken);

        if (result.IsFailed)
            return result.ToResult<EntityDefinition?>();

        return result.Value is null
            ? Result.Ok<EntityDefinition?>(null)
            : Result.Ok<EntityDefinition?>(_mapper.Map<EntityDefinition>(result.Value));
    }

    public async Task<Result<IReadOnlyList<EntityDefinition>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var filter = NotDeleted;
        var result = await _repository.GetListAsync(filter, cancellationToken);

        if (result.IsFailed)
            return result.ToResult<IReadOnlyList<EntityDefinition>>();

        var documents = result.Value ?? [];
        return Result.Ok<IReadOnlyList<EntityDefinition>>(
            documents.Select(_mapper.Map<EntityDefinition>).ToList());
    }

    public async Task<Result> DeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<EntityDefinitionDocument>.Filter.Eq(x => x.Id, id);
        var result = await _repository.DeleteAsync(filter, cancellationToken);
        return result.ToResult();
    }

    public async Task<Result<bool>> ExistsBySchemaNameAsync(
        string schemaName, CancellationToken cancellationToken = default)
    {
        var filter = Builders<EntityDefinitionDocument>.Filter.Eq(x => x.SchemaName, schemaName)
                     & NotDeleted;
        var result = await _repository.GetAsync(filter, cancellationToken);

        if (result.IsFailed)
            return result.ToResult<bool>();

        return Result.Ok(result.Value is not null);
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

        var filter = BuildFilter(searchTerm, isAbstract, tags);

        var countResult = await _repository.CountAsync(filter, cancellationToken);
        if (countResult.IsFailed)
            return countResult.ToResult<(IReadOnlyList<EntityDefinition>, long)>();

        var totalCount = countResult.Value;

        // TODO: Replace with server-side Skip/Limit once IGenericRepository supports paged queries.
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
                break;
        }

        return Result.Ok<(IReadOnlyList<EntityDefinition>, long)>((items, totalCount));
    }

    private static FilterDefinition<EntityDefinitionDocument> BuildFilter(
        string? searchTerm, bool? isAbstract, IReadOnlyList<string>? tags)
    {
        var filters = new List<FilterDefinition<EntityDefinitionDocument>> { NotDeleted };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            filters.Add(Builders<EntityDefinitionDocument>.Filter.Regex(
                x => x.DisplayName,
                new MongoDB.Bson.BsonRegularExpression(Regex.Escape(searchTerm), "i")));
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

        return Builders<EntityDefinitionDocument>.Filter.And(filters);
    }
}
