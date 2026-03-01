using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain;
using Microsoft.Extensions.Logging;

namespace Dilcore.Blueprints.Actors;

public class EntityDefinitionGrain : Grain, IEntityDefinitionGrain
{
    private readonly IPersistentState<EntityDefinitionState> _state;
    private readonly ILogger<EntityDefinitionGrain> _logger;
    private readonly TimeProvider _timeProvider;

    public EntityDefinitionGrain(
        [PersistentState("entityDefinition", SiloBuilderExtensions.StoreName)]
        IPersistentState<EntityDefinitionState> state,
        ILogger<EntityDefinitionGrain> logger,
        TimeProvider timeProvider)
    {
        _state = state;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _logger.LogEntityDefinitionGrainActivating(this.GetPrimaryKey());
        return base.OnActivateAsync(cancellationToken);
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        _logger.LogEntityDefinitionGrainDeactivating(this.GetPrimaryKey(), reason.ToString());
        return base.OnDeactivateAsync(reason, cancellationToken);
    }

    public async Task<EntityDefinitionGrainResult> CreateAsync(CreateEntityDefinitionGrainCommand command)
    {
        var grainId = this.GetPrimaryKey();

        if (_state.State.IsCreated)
        {
            _logger.LogEntityDefinitionAlreadyExists(grainId);
            return EntityDefinitionGrainResult.NotFound($"Entity definition '{grainId}' already exists.");
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        _state.State.Id = grainId;
        _state.State.SchemaName = SchemaNameGenerator.Generate(command.DisplayName);
        _state.State.DisplayName = command.DisplayName;
        _state.State.Description = command.Description;
        _state.State.IsAbstract = command.IsAbstract;
        _state.State.ExtendsEntityId = command.ExtendsEntityId;
        _state.State.Fields = GenerateFieldSchemaNames(command.Fields);
        _state.State.Tags = command.Tags;
        _state.State.CreatedAt = now;
        _state.State.UpdatedAt = now;
        _state.State.IsCreated = true;

        await _state.WriteStateAsync();

        _logger.LogEntityDefinitionCreated(grainId, _state.State.SchemaName);

        return EntityDefinitionGrainResult.Success(ToDto());
    }

    public Task<EntityDefinitionGrainDto?> GetAsync()
    {
        if (!_state.State.IsCreated)
        {
            _logger.LogEntityDefinitionNotFound(this.GetPrimaryKey());
            return Task.FromResult<EntityDefinitionGrainDto?>(null);
        }

        return Task.FromResult<EntityDefinitionGrainDto?>(ToDto());
    }

    public async Task<EntityDefinitionGrainResult> UpdateAsync(UpdateEntityDefinitionGrainCommand command)
    {
        var grainId = this.GetPrimaryKey();

        if (!_state.State.IsCreated)
        {
            _logger.LogEntityDefinitionNotFound(grainId);
            return EntityDefinitionGrainResult.NotFound($"Entity definition '{grainId}' does not exist.");
        }

        if (command.ETag != _state.State.ETag)
        {
            _logger.LogEntityDefinitionETagMismatch(grainId, command.ETag, _state.State.ETag);
            return EntityDefinitionGrainResult.ETagMismatch(
                $"ETag mismatch for entity definition '{grainId}'. Expected {_state.State.ETag}, got {command.ETag}.");
        }

        _state.State.Description = command.Description;
        _state.State.IsAbstract = command.IsAbstract;
        _state.State.ExtendsEntityId = command.ExtendsEntityId;
        _state.State.Fields = GenerateFieldSchemaNames(command.Fields);
        _state.State.Tags = command.Tags;
        _state.State.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;

        await _state.WriteStateAsync();

        _logger.LogEntityDefinitionUpdated(grainId, _state.State.SchemaName);

        return EntityDefinitionGrainResult.Success(ToDto());
    }

    public async Task<EntityDefinitionGrainResult> DeleteAsync()
    {
        var grainId = this.GetPrimaryKey();

        if (!_state.State.IsCreated)
        {
            _logger.LogEntityDefinitionNotFound(grainId);
            return EntityDefinitionGrainResult.NotFound($"Entity definition '{grainId}' does not exist.");
        }

        var dto = ToDto();

        _state.State.IsCreated = false;
        await _state.ClearStateAsync();

        _logger.LogEntityDefinitionDeleted(grainId, dto.SchemaName);

        DeactivateOnIdle();

        return EntityDefinitionGrainResult.Success(dto);
    }

    private static List<FieldDefinitionGrainDto> GenerateFieldSchemaNames(List<FieldDefinitionGrainDto> fields) =>
        fields.Select(f => f with
        {
            SchemaName = SchemaNameGenerator.Generate(f.DisplayName),
            Fields = f.Fields is { Count: > 0 }
                ? GenerateFieldSchemaNames(f.Fields)
                : f.Fields
        }).ToList();

    private EntityDefinitionGrainDto ToDto() => new()
    {
        Id = _state.State.Id,
        SchemaName = _state.State.SchemaName,
        DisplayName = _state.State.DisplayName,
        Description = _state.State.Description,
        IsAbstract = _state.State.IsAbstract,
        ExtendsEntityId = _state.State.ExtendsEntityId,
        Fields = _state.State.Fields,
        Tags = _state.State.Tags,
        CreatedAt = _state.State.CreatedAt,
        UpdatedAt = _state.State.UpdatedAt,
        ETag = _state.State.ETag
    };
}
