using AutoMapper;
using Dilcore.Blueprints.Core.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Domain.Abstractions;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Storage;

namespace Dilcore.Blueprints.Actors.Storage;

public sealed class BlueprintDefinitionStorage : IGrainStorage
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<BlueprintDefinitionStorage> _logger;

    private static readonly Dictionary<Type, IStateStorageHandler> Handlers = new()
    {
        [typeof(EntityDefinitionState)] = new StateStorageHandler<EntityDefinitionState, EntityDefinition, IEntityDefinitionRepository>(
            (repo, id) => repo.GetByIdAsync(id),
            (repo, entity) => repo.StoreAsync(entity),
            (repo, id) => repo.DeleteAsync(id))
    };

    public BlueprintDefinitionStorage(
        IServiceScopeFactory scopeFactory,
        IMapper mapper,
        ILogger<BlueprintDefinitionStorage> logger)
    {
        _scopeFactory = scopeFactory;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var handler = ResolveHandler(typeof(T));
        var id = grainId.GetGuidKey();

        _logger.LogReadingState(typeof(T).Name, id);

        using var scope = _scopeFactory.CreateScope();
        var result = await handler.ReadAsync(scope.ServiceProvider, _mapper, id);

        if (result.IsFailed)
        {
            _logger.LogReadStateError(null, typeof(T).Name, id);
            throw new InvalidOperationException(
                $"Failed to read {typeof(T).Name} '{id}': {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        if (result.Value is null)
        {
            _logger.LogStateNotFoundForRead(typeof(T).Name, id);
            grainState.State = Activator.CreateInstance<T>();
            grainState.RecordExists = false;
            grainState.ETag = null;
            return;
        }

        grainState.State = _mapper.Map<T>(result.Value);
        grainState.RecordExists = true;
        grainState.ETag = ((BaseDomain)result.Value).ETag.ToString();

        _logger.LogStateLoaded(typeof(T).Name, id);
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var handler = ResolveHandler(typeof(T));
        var id = grainId.GetGuidKey();

        _logger.LogWritingState(typeof(T).Name, id);

        var domainEntity = handler.MapToDomain(_mapper, grainState.State!);

        using var scope = _scopeFactory.CreateScope();
        var result = await handler.WriteAsync(scope.ServiceProvider, _mapper, domainEntity);

        if (result.IsFailed)
        {
            _logger.LogWriteStateError(null, typeof(T).Name, id);
            throw new InvalidOperationException(
                $"Failed to write {typeof(T).Name} '{id}': {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        grainState.State = _mapper.Map<T>(result.Value);
        grainState.RecordExists = true;
        grainState.ETag = ((BaseDomain)result.Value).ETag.ToString();

        _logger.LogStateWritten(typeof(T).Name, id);
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var handler = ResolveHandler(typeof(T));
        var id = grainId.GetGuidKey();

        _logger.LogClearingState(typeof(T).Name, id);

        using var scope = _scopeFactory.CreateScope();
        await handler.DeleteAsync(scope.ServiceProvider, id);
    }

    private static IStateStorageHandler ResolveHandler(Type stateType)
    {
        if (!Handlers.TryGetValue(stateType, out var handler))
            throw new InvalidOperationException($"No storage handler registered for state type '{stateType.Name}'.");

        return handler;
    }
}

internal interface IStateStorageHandler
{
    Task<Result<BaseDomain?>> ReadAsync(IServiceProvider services, IMapper mapper, Guid id);
    BaseDomain MapToDomain(IMapper mapper, object state);
    Task<Result<BaseDomain>> WriteAsync(IServiceProvider services, IMapper mapper, BaseDomain entity);
    Task DeleteAsync(IServiceProvider services, Guid id);
}

internal sealed class StateStorageHandler<TState, TDomain, TRepository>
    : IStateStorageHandler
    where TDomain : BaseDomain
    where TRepository : notnull
{
    private readonly Func<TRepository, Guid, Task<Result<TDomain?>>> _read;
    private readonly Func<TRepository, TDomain, Task<Result<TDomain>>> _write;
    private readonly Func<TRepository, Guid, Task<Result>>? _delete;

    public StateStorageHandler(
        Func<TRepository, Guid, Task<Result<TDomain?>>> read,
        Func<TRepository, TDomain, Task<Result<TDomain>>> write,
        Func<TRepository, Guid, Task<Result>>? delete = null)
    {
        _read = read;
        _write = write;
        _delete = delete;
    }

    public async Task<Result<BaseDomain?>> ReadAsync(IServiceProvider services, IMapper mapper, Guid id)
    {
        var repository = services.GetRequiredService<TRepository>();
        var result = await _read(repository, id);

        return result.IsSuccess
            ? Result.Ok<BaseDomain?>(result.Value)
            : Result.Fail<BaseDomain?>(result.Errors);
    }

    public BaseDomain MapToDomain(IMapper mapper, object state) =>
        mapper.Map<TDomain>((TState)state);

    public async Task<Result<BaseDomain>> WriteAsync(IServiceProvider services, IMapper mapper, BaseDomain entity)
    {
        var repository = services.GetRequiredService<TRepository>();
        var result = await _write(repository, (TDomain)entity);

        return result.IsSuccess
            ? Result.Ok<BaseDomain>(result.Value)
            : Result.Fail<BaseDomain>(result.Errors);
    }

    public async Task DeleteAsync(IServiceProvider services, Guid id)
    {
        if (_delete is null) return;
        var repository = services.GetRequiredService<TRepository>();
        await _delete(repository, id);
    }
}
