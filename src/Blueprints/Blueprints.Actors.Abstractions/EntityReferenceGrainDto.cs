namespace Dilcore.Blueprints.Actors.Abstractions;

[GenerateSerializer]
[Immutable]
public sealed record EntityReferenceGrainDto
{
    [Id(0)]
    public required string SchemaName { get; init; }

    [Id(1)]
    public required string ReferenceType { get; init; }

    [Id(2)]
    public Guid RelatedEntityDefinitionId { get; init; }

    [Id(3)]
    public required string RelatedEntitySchemaName { get; init; }
}
