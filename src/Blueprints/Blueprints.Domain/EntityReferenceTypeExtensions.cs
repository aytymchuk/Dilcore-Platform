namespace Dilcore.Blueprints.Domain.Entities;

public static class EntityReferenceTypeExtensions
{
    public static EntityReferenceType GetInverse(this EntityReferenceType type) => type switch
    {
        EntityReferenceType.OneToOne => EntityReferenceType.OneToOne,
        EntityReferenceType.OneToMany => EntityReferenceType.ManyToOne,
        EntityReferenceType.ManyToOne => EntityReferenceType.OneToMany,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown reference type.")
    };
}
