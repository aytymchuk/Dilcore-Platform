using Dilcore.Blueprints.Actors.Abstractions;

namespace Dilcore.Blueprints.Actors;

public static class EntityReferenceCollectionExtensions
{
    public static int FindIndexBySchemaName(this IReadOnlyList<EntityReferenceGrainDto> references, string schemaName)
    {
        for (var i = 0; i < references.Count; i++)
        {
            if (references[i].SchemaName.Equals(schemaName, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    public static int FindIndexByRelatedEntityId(this IReadOnlyList<EntityReferenceGrainDto> references, Guid relatedEntityId)
    {
        for (var i = 0; i < references.Count; i++)
        {
            if (references[i].RelatedEntityDefinitionId == relatedEntityId)
            {
                return i;
            }
        }

        return -1;
    }

    public static bool ContainsSchemaName(this IEnumerable<EntityReferenceGrainDto> references, string schemaName) =>
        references.Any(r => r.SchemaName.Equals(schemaName, StringComparison.OrdinalIgnoreCase));
}
