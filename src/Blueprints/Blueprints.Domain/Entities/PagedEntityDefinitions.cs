namespace Dilcore.Blueprints.Domain.Entities;

public record PagedEntityDefinitions(
    IReadOnlyList<EntityDefinition> Items,
    long TotalCount);
