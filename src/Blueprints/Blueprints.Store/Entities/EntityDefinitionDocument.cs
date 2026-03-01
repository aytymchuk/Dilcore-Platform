using Dilcore.Blueprints.Store.Entities.Fields;
using Dilcore.DocumentDb.Abstractions;

namespace Dilcore.Blueprints.Store.Entities;

public sealed class EntityDefinitionDocument : IDocumentEntity
{
    public Guid Id { get; set; }
    public long ETag { get; set; }
    public bool IsDeleted { get; set; }
    /// <summary>Always stored as UTC (<see cref="DateTimeKind.Utc"/>).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Always stored as UTC (<see cref="DateTimeKind.Utc"/>).</summary>
    public DateTime UpdatedAt { get; set; }

    public required string SchemaName { get; set; }
    public required string DisplayName { get; set; }
    public string? Description { get; set; }
    public bool IsAbstract { get; set; }
    public Guid? ExtendsEntityId { get; set; }

    public List<FieldDefinitionDocument> Fields { get; set; } = [];
    public EntityMetadataDocument Metadata { get; set; } = new();
}
