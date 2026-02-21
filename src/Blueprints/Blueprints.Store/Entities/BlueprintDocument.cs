using Dilcore.DocumentDb.Abstractions;

namespace Dilcore.Blueprints.Store.Entities;

/// <summary>
/// MongoDB document entity for the Blueprints module. Extend with Blueprint-specific properties when adding domain models.
/// </summary>
public sealed class BlueprintDocument : IDocumentEntity
{
    public Guid Id { get; set; }
    public long ETag { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
