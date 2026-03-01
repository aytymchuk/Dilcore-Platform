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
    public List<FieldDefinitionGrainDto> Fields { get; init; } = [];

    [Id(5)]
    public List<string> Tags { get; init; } = [];
}
