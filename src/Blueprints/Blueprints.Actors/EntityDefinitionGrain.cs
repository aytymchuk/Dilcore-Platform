using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain;
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

            var addRefsResult = await AddForwardReferencesToStateAsync(command.References, entitySchemaName, grainId);
            if (!addRefsResult.IsSuccess)
                return addRefsResult;

            await _state.WriteStateAsync();

            _logger.LogEntityDefinitionCreated(grainId, _state.State.SchemaName);

            return await AddReverseReferencesFromCreateAsync(command.References, entitySchemaName, grainId);
        }
        catch (ArgumentException ex)
        {
            return EntityDefinitionGrainResult.Validation(ex.Message);
        }
    }

    private async Task<EntityDefinitionGrainResult> AddForwardReferencesToStateAsync(
        EntityReferenceGrainParameter[] references,
        string entitySchemaName,
        Guid grainId)
    {
        if (references is null or { Length: 0 })
            return EntityDefinitionGrainResult.Success(_state.State.ToGrainDto());

        foreach (var reference in references)
        {
            var targetGrain = GrainFactory.GetGrain<IEntityDefinitionGrain>(reference.RelatedEntityDefinitionId);
            var targetDto = await targetGrain.GetAsync();

            if (targetDto is null)
            {
                return EntityDefinitionGrainResult.Validation(
                    $"Referenced entity definition '{reference.RelatedEntityDefinitionId}' does not exist.");
            }

            var addRefCommand = reference.ToAddReferenceCommand(targetDto.SchemaName);

            var (validationError, refDto) = ValidateAndCreateReferenceDto(addRefCommand);
            if (validationError is not null)
                return validationError;

            _state.State.References.Add(refDto!);

            if (reference.RelatedEntityDefinitionId == grainId)
            {
                var reverseCommand = reference.ToReverseCommand(entitySchemaName, grainId, skipReverseReference: false);
                _state.State.References.Add(EntityReferenceCommandExtensions.ToEntityReferenceGrainDto(entitySchemaName, reverseCommand));
            }
        }

        _state.State.TouchUpdatedAt(_timeProvider);
        return EntityDefinitionGrainResult.Success(_state.State.ToGrainDto());
    }

    private async Task<EntityDefinitionGrainResult> AddReverseReferencesFromCreateAsync(
        EntityReferenceGrainParameter[] references,
        string entitySchemaName,
        Guid grainId)
    {
        if (references is null or { Length: 0 })
            return EntityDefinitionGrainResult.Success(_state.State.ToGrainDto());

        var errors = new List<string>();

        foreach (var reference in references)
        {
            if (reference.RelatedEntityDefinitionId == grainId)
                continue;

            var reverseCommand = reference.ToReverseCommand(entitySchemaName, grainId);

            var targetGrain = GrainFactory.GetGrain<IEntityDefinitionGrain>(reference.RelatedEntityDefinitionId);
            var reverseResult = await targetGrain.AddReferenceAsync(reverseCommand);

            if (!reverseResult.IsSuccess)
            {
                errors.Add($"'{reference.RelatedEntityDefinitionId}': {reverseResult.ErrorMessage ?? "Failed to add reverse reference."}");
            }
        }

        return errors.Count > 0
            ? EntityDefinitionGrainResult.Validation(string.Join("; ", errors))
            : EntityDefinitionGrainResult.Success(_state.State.ToGrainDto());
    }

    public Task<EntityDefinitionGrainDto?> GetAsync()
    {
        if (!_state.State.IsCreated)
        {
            _logger.LogEntityDefinitionNotFound(this.GetPrimaryKey());
            return Task.FromResult<EntityDefinitionGrainDto?>(null);
        }

        return Task.FromResult<EntityDefinitionGrainDto?>(_state.State.ToGrainDto());
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
            _state.State.TouchUpdatedAt(_timeProvider);

            await _state.WriteStateAsync();

            _logger.LogEntityDefinitionUpdated(grainId, _state.State.SchemaName);

            return EntityDefinitionGrainResult.Success(
                _state.State.ToGrainDto(), LastFieldChanges.Added, LastFieldChanges.Removed);
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

        var dto = _state.State.ToGrainDto();

        _state.State.IsCreated = false;
        await _state.ClearStateAsync();

        _logger.LogEntityDefinitionDeleted(grainId, dto.SchemaName);

        DeactivateOnIdle();

        return EntityDefinitionGrainResult.Success(dto);
    }

    public async Task<EntityDefinitionGrainResult> AddReferenceAsync(AddEntityReferenceGrainCommand command)
    {
        var grainId = this.GetPrimaryKey();

        if (!_state.State.IsCreated)
        {
            _logger.LogEntityDefinitionNotFound(grainId);
            return EntityDefinitionGrainResult.NotFound($"Entity definition '{grainId}' does not exist.");
        }

        var (validationError, reference) = ValidateAndCreateReferenceDto(command);
        if (validationError is not null)
            return validationError;

        _state.State.References.Add(reference!);
        _state.State.TouchUpdatedAt(_timeProvider);

        await _state.WriteStateAsync();

        _logger.LogEntityDefinitionReferenceAdded(grainId, reference!.SchemaName);

        if (!command.SkipReverseReference)
            return await AddReverseReferenceAsync(command, grainId);

        return EntityDefinitionGrainResult.Success(_state.State.ToGrainDto());
    }

    public async Task<EntityDefinitionGrainResult> RemoveReferenceAsync(RemoveEntityReferenceGrainCommand command)
    {
        var grainId = this.GetPrimaryKey();

        if (!_state.State.IsCreated)
        {
            _logger.LogEntityDefinitionNotFound(grainId);
            return EntityDefinitionGrainResult.NotFound($"Entity definition '{grainId}' does not exist.");
        }

        var index = _state.State.References.FindIndexBySchemaName(command.SchemaName);

        if (index < 0)
            return EntityDefinitionGrainResult.NotFound(
                $"Reference with schema name '{command.SchemaName}' does not exist on this entity.");

        var removedReference = _state.State.References[index];
        _state.State.References.RemoveAt(index);
        _state.State.TouchUpdatedAt(_timeProvider);

        await _state.WriteStateAsync();

        _logger.LogEntityDefinitionReferenceRemoved(grainId, command.SchemaName);

        if (!command.SkipReverseReference)
        {
            if (removedReference.RelatedEntityDefinitionId == grainId)
            {
                var reverseIndex = _state.State.References.FindIndexBySchemaName(_state.State.SchemaName);
                if (reverseIndex >= 0)
                {
                    _state.State.References.RemoveAt(reverseIndex);
                    _state.State.TouchUpdatedAt(_timeProvider);
                    await _state.WriteStateAsync();
                    _logger.LogEntityDefinitionReferenceRemoved(grainId, _state.State.SchemaName);
                }
            }
            else
            {
                var targetGrain = GrainFactory.GetGrain<IEntityDefinitionGrain>(removedReference.RelatedEntityDefinitionId);
                var reverseResult = await targetGrain.RemoveReferenceAsync(
                    new RemoveEntityReferenceGrainCommand { SchemaName = _state.State.SchemaName, SkipReverseReference = true });
                
                if (!reverseResult.IsSuccess && reverseResult.ErrorCode != EntityDefinitionGrainResult.NotFoundCode)
                    return EntityDefinitionGrainResult.Validation(
                        reverseResult.ErrorMessage ?? "Failed to remove reverse reference.");
            }
        }

        return EntityDefinitionGrainResult.Success(_state.State.ToGrainDto(), removedReference);
    }

    private async Task<EntityDefinitionGrainResult> AddReverseReferenceAsync(
        AddEntityReferenceGrainCommand command, Guid grainId)
    {
        var reverseCommand = command.ToReverseCommand(_state.State.SchemaName, grainId);

        if (command.RelatedEntityDefinitionId == grainId)
        {
            return await AddReferenceAsync(reverseCommand);
        }

        var targetGrain = GrainFactory.GetGrain<IEntityDefinitionGrain>(command.RelatedEntityDefinitionId);
        var reverseResult = await targetGrain.AddReferenceAsync(reverseCommand);
        return reverseResult.IsSuccess
            ? EntityDefinitionGrainResult.Success(_state.State.ToGrainDto())
            : EntityDefinitionGrainResult.Validation(
                reverseResult.ErrorMessage ?? "Failed to add reverse reference.");
    }

    private (EntityDefinitionGrainResult? Error, EntityReferenceGrainDto? Dto) ValidateAndCreateReferenceDto(
        AddEntityReferenceGrainCommand command)
    {
        if (_state.State.References.Count >= EntityDefinitionLimits.MaxReferencesPerEntity)
            return (EntityDefinitionGrainResult.Validation(
                $"Entity definition cannot have more than {EntityDefinitionLimits.MaxReferencesPerEntity} references."), null);

        var (schemaNameError, referenceSchemaName) = ResolveReferenceSchemaName(command);
        if (schemaNameError is not null)
            return (schemaNameError, null);

        return (null, EntityReferenceCommandExtensions.ToEntityReferenceGrainDto(referenceSchemaName!, command));
    }

    private (EntityDefinitionGrainResult? Error, string? SchemaName) ResolveReferenceSchemaName(AddEntityReferenceGrainCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.SchemaName))
        {
            var generatedSchemaName = EntityReferenceSchemaNameExtensions.GenerateReferenceSchemaName(
                _state.State.SchemaName,
                command.RelatedEntitySchemaName);

            return FindUniqueReferenceSchemaName(generatedSchemaName);
        }

        var schemaNameResult = SchemaNameGenerator.SafeResolve(command.SchemaName, command.RelatedEntitySchemaName);
        if (schemaNameResult.IsFailed)
            return (EntityDefinitionGrainResult.Validation(schemaNameResult.Errors.First().Message), null);

        return FindUniqueReferenceSchemaName(schemaNameResult.Value);
    }

    private (EntityDefinitionGrainResult? Error, string? SchemaName) FindUniqueReferenceSchemaName(string baseSchemaName)
    {
        if (!SchemaNameGenerator.IsValid(baseSchemaName))
        {
            return (EntityDefinitionGrainResult.Validation(
                $"Reference schema name '{baseSchemaName}' is not valid. Schema names must be camelCase starting with a lowercase letter and cannot use reserved names."), null);
        }

        if (!_state.State.References.ContainsSchemaName(baseSchemaName))
        {
            return (null, baseSchemaName);
        }

        for (var suffix = 2; suffix <= EntityDefinitionLimits.MaxReferencesPerEntity; suffix++)
        {
            var candidate = $"{baseSchemaName}{suffix}";

            if (!SchemaNameGenerator.IsValid(candidate))
                continue;

            if (!_state.State.References.ContainsSchemaName(candidate))
            {
                return (null, candidate);
            }
        }

        return (EntityDefinitionGrainResult.Validation(
            $"Unable to generate a unique schema name for reference '{baseSchemaName}'."), null);
    }
}

internal record FieldChanges(IReadOnlyList<string> Added, IReadOnlyList<string> Removed);
