using AutoMapper;
using Dilcore.Blueprints.Core.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Blueprints.Store.Entities;
using Dilcore.DocumentDb.MongoDb.Repositories.Abstractions;
using FluentResults;
using MongoDB.Driver;

namespace Dilcore.Blueprints.Store.Repositories;

public sealed class EntityDefinitionRepository : IEntityDefinitionRepository
{
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
        var document = _mapper.Map<EntityDefinitionDocument>(entity);
        var result = await _repository.StoreAsync(document, cancellationToken);

        return result.IsFailed
            ? result.ToResult<EntityDefinition>()
            : Result.Ok(_mapper.Map<EntityDefinition>(result.Value));
    }

    public async Task<Result<EntityDefinition?>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<EntityDefinitionDocument>.Filter.Eq(x => x.Id, id);
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
        var filter = Builders<EntityDefinitionDocument>.Filter.Empty;
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
        var filter = Builders<EntityDefinitionDocument>.Filter.Eq(x => x.SchemaName, schemaName);
        var result = await _repository.GetAsync(filter, cancellationToken);

        if (result.IsFailed)
            return result.ToResult<bool>();

        return Result.Ok(result.Value is not null);
    }
}
