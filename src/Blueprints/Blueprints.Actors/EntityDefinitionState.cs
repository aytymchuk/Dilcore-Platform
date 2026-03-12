using Dilcore.Blueprints.Actors.Abstractions;

namespace Dilcore.Blueprints.Actors;

[GenerateSerializer]
public sealed class EntityDefinitionState
{
    [Id(0)]
    public Guid Id { get; set; }

    [Id(1)]
    public long ETag { get; set; }

    [Id(2)]
    public string SchemaName { get; set; } = string.Empty;

    [Id(3)]
    public string DisplayName { get; set; } = string.Empty;

    [Id(4)]
    public string? Description { get; set; }

    [Id(5)]
    public bool IsAbstract { get; set; }

    [Id(6)]
    public Guid? ExtendsEntityId { get; set; }

    [Id(7)]
    public List<FieldDefinitionGrainDto> Fields { get; set; } = [];

    [Id(8)]
    public List<string> Tags { get; set; } = [];

    [Id(9)]
    public DateTime CreatedAt { get; set; }

    [Id(10)]
    public DateTime UpdatedAt { get; set; }

    [Id(11)]
    public bool IsCreated { get; set; }

    [Id(12)]
    public List<EntityReferenceGrainDto> References { get; set; } = [];
}
