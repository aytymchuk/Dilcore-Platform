namespace Dilcore.Blueprints.Actors.Abstractions;

[GenerateSerializer]
[Immutable]
[Alias("Dilcore.Blueprints.Actors.Abstractions.EntityDefinitionGrainDto")]
public sealed record EntityDefinitionGrainDto
{
    [Id(0)]
    public Guid Id { get; init; }

    [Id(1)]
    public long ETag { get; init; }

    [Id(2)]
    public required string SchemaName { get; init; }

    [Id(3)]
    public required string DisplayName { get; init; }

    [Id(4)]
    public string? Description { get; init; }

    [Id(5)]
    public bool IsAbstract { get; init; }

    [Id(6)]
    public Guid? ExtendsEntityId { get; init; }

    [Id(7)]
    public FieldDefinitionGrainDto[] Fields { get; init; } = [];

    [Id(8)]
    public string[] Tags { get; init; } = [];

    [Id(9)]
    public DateTime CreatedAt { get; init; }

    [Id(10)]
    public DateTime UpdatedAt { get; init; }
}
