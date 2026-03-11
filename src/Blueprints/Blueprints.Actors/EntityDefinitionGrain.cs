using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Dilcore.Blueprints.Actors;

public class EntityDefinitionGrain : Grain, IEntityDefinitionGrain
{
    private readonly IPersistentState<EntityDefinitionState> _state;
    private readonly ILogger<EntityDefinitionGrain> _logger;
    private readonly TimeProvider _timeProvider;

    internal FieldChanges? LastFieldChanges { get; private set; }

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
            return EntityDefinitionGrainResult.AlreadyExists($"Entity definition '{grainId}' already exists.");
        }

        try
        {
            var fields = FieldSchemaProcessor.GenerateSchemaNames(command.Fields);

            var schemaNameResult = SchemaNameGenerator.SafeResolve(command.SchemaName, command.DisplayName);
            if (schemaNameResult.IsFailed)
                return EntityDefinitionGrainResult.Validation(schemaNameResult.Errors.First().Message);

            var entitySchemaName = schemaNameResult.Value;

            if (!SchemaNameGenerator.IsValid(entitySchemaName))
                return EntityDefinitionGrainResult.Validation(
                    $"Schema name '{entitySchemaName}' is not valid. Entity schema names must be camelCase starting with a lowercase letter and cannot use reserved names.");

            var fieldError = EntityDefinitionValidator.ValidateFields(fields)
                ?? FieldSchemaProcessor.ValidateSchemaNames(fields, entitySchemaName);

            if (fieldError is not null)
                return EntityDefinitionGrainResult.Validation(fieldError);

            var now = _timeProvider.GetUtcNow().UtcDateTime;

            _state.State.Id = grainId;
            _state.State.SchemaName = entitySchemaName;
            _state.State.DisplayName = command.DisplayName;
            _state.State.Description = command.Description;
            _state.State.IsAbstract = command.IsAbstract;
            _state.State.ExtendsEntityId = command.ExtendsEntityId;
            _state.State.Fields = fields;
            _state.State.Tags = command.Tags.ToList();
            _state.State.CreatedAt = now;
            _state.State.UpdatedAt = now;
            _state.State.IsCreated = true;

            await _state.WriteStateAsync();

            _logger.LogEntityDefinitionCreated(grainId, _state.State.SchemaName);

            return EntityDefinitionGrainResult.Success(ToDto());
        }
        catch (ArgumentException ex)
        {
            return EntityDefinitionGrainResult.Validation(ex.Message);
        }
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
            _logger.LogEntityDefinitionETagMismatch(grainId, _state.State.ETag, command.ETag);
            return EntityDefinitionGrainResult.ETagMismatch(
                $"ETag mismatch for entity definition '{grainId}'. Expected {_state.State.ETag}, got {command.ETag}.");
        }

        try
        {
            var newFields = FieldSchemaProcessor.MergeWithExisting(command.Fields, _state.State.Fields);

            var fieldError = EntityDefinitionValidator.ValidateFields(newFields)
                ?? FieldSchemaProcessor.ValidateSchemaNames(newFields, _state.State.SchemaName);

            if (fieldError is not null)
                return EntityDefinitionGrainResult.Validation(fieldError);

            LastFieldChanges = FieldSchemaProcessor.ComputeChanges(_state.State.Fields, newFields);

            if (!string.IsNullOrEmpty(command.DisplayName))
                _state.State.DisplayName = command.DisplayName;

            _state.State.Description = command.Description;
            _state.State.IsAbstract = command.IsAbstract;
            _state.State.Fields = newFields;
            _state.State.Tags = command.Tags.ToList();
            _state.State.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;

            await _state.WriteStateAsync();

            _logger.LogEntityDefinitionUpdated(grainId, _state.State.SchemaName);

            return EntityDefinitionGrainResult.Success(
                ToDto(), LastFieldChanges.Added, LastFieldChanges.Removed);
        }
        catch (ArgumentException ex)
        {
            return EntityDefinitionGrainResult.Validation(ex.Message);
        }
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

    private EntityDefinitionGrainDto ToDto() => new()
    {
        Id = _state.State.Id,
        SchemaName = _state.State.SchemaName,
        DisplayName = _state.State.DisplayName,
        Description = _state.State.Description,
        IsAbstract = _state.State.IsAbstract,
        ExtendsEntityId = _state.State.ExtendsEntityId,
        Fields = _state.State.Fields.ToArray(),
        Tags = _state.State.Tags.ToArray(),
        CreatedAt = _state.State.CreatedAt,
        UpdatedAt = _state.State.UpdatedAt,
        ETag = _state.State.ETag
    };
}

internal record FieldChanges(IReadOnlyList<string> Added, IReadOnlyList<string> Removed);
