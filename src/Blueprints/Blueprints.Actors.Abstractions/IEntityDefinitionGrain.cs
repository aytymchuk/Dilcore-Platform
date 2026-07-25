namespace Dilcore.Blueprints.Actors.Abstractions;

/// <summary>
/// Orleans grain representing an entity definition in the Blueprint system.
/// Grain key is the entity definition Id (Guid).
/// </summary>
public interface IEntityDefinitionGrain : IGrainWithGuidKey
{
    Task<EntityDefinitionGrainResult> CreateAsync(CreateEntityDefinitionGrainCommand command);

    Task<EntityDefinitionGrainDto?> GetAsync();

    Task<EntityDefinitionGrainResult> UpdateAsync(UpdateEntityDefinitionGrainCommand command);

    Task<EntityDefinitionGrainResult> DeleteAsync();

    Task<EntityDefinitionGrainResult> AddReferenceAsync(AddEntityReferenceGrainCommand command);

    Task<EntityDefinitionGrainResult> RemoveReferenceAsync(RemoveEntityReferenceGrainCommand command);
}
