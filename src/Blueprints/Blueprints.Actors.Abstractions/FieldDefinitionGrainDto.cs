namespace Dilcore.Blueprints.Actors.Abstractions;

[GenerateSerializer]
[Immutable]
[Alias("Dilcore.Blueprints.Actors.Abstractions.FieldDefinitionGrainDto")]
public sealed record FieldDefinitionGrainDto
{
    [Id(0)]
    public required string SchemaName { get; init; }

    [Id(1)]
    public required string DisplayName { get; init; }

    [Id(2)]
    public required string Type { get; init; }

    [Id(3)]
    public FieldDefinitionGrainDto[]? Fields { get; init; }
}
