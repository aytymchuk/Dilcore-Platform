namespace Dilcore.Blueprints.Actors.Abstractions;

[GenerateSerializer]
[Immutable]
public sealed record AddEntityReferenceGrainCommand
{
    [Id(0)]
    public string? SchemaName { get; init; }

    [Id(1)]
    public required string ReferenceType { get; init; }

    [Id(2)]
    public required Guid RelatedEntityDefinitionId { get; init; }

    [Id(3)]
    public required string RelatedEntitySchemaName { get; init; }

    [Id(4)]
    public bool SkipReverseReference { get; init; }
}
