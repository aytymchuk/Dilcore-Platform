namespace Dilcore.Blueprints.Actors.Abstractions;

[GenerateSerializer]
[Immutable]
public sealed record CreateEntityDefinitionGrainCommand
{
    [Id(0)]
    public required string DisplayName { get; init; }

    [Id(1)]
    public string? Description { get; init; }

    [Id(2)]
    public bool IsAbstract { get; init; }

    [Id(3)]
    public Guid? ExtendsEntityId { get; init; }

    [Id(4)]
    public FieldDefinitionGrainDto[] Fields { get; init; } = [];

    [Id(5)]
    public string[] Tags { get; init; } = [];

    [Id(6)]
    public string? SchemaName { get; init; }

    [Id(7)]
    public EntityReferenceGrainParameter[] References { get; init; } = [];
}
