using Dilcore.Blueprints.Actors.Abstractions;

namespace Dilcore.Blueprints.Actors;

public static class EntityDefinitionStateExtensions
{
    public static void TouchUpdatedAt(this EntityDefinitionState state, TimeProvider timeProvider)
    {
        state.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    public static EntityDefinitionGrainDto ToGrainDto(this EntityDefinitionState state) => new()
    {
        Id = state.Id,
        SchemaName = state.SchemaName,
        DisplayName = state.DisplayName,
        Description = state.Description,
        IsAbstract = state.IsAbstract,
        ExtendsEntityId = state.ExtendsEntityId,
        Fields = state.Fields.ToArray(),
        References = state.References.ToArray(),
        Tags = state.Tags.ToArray(),
        CreatedAt = state.CreatedAt,
        UpdatedAt = state.UpdatedAt,
        ETag = state.ETag
    };
}
