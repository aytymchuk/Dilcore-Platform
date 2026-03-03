namespace Dilcore.Blueprints.Actors.Abstractions;

[GenerateSerializer]
[Immutable]
public sealed record UpdateEntityDefinitionGrainCommand
{
    [Id(0)]
    public long ETag { get; init; }

    [Id(1)]
    public string? Description { get; init; }

    [Id(2)]
    public bool IsAbstract { get; init; }

    [Id(3)]
    public string? DisplayName { get; init; }

    [Id(4)]
    public FieldDefinitionGrainDto[] Fields { get; init; } = [];

    [Id(5)]
    public string[] Tags { get; init; } = [];
}
