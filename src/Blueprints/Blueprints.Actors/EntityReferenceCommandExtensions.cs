using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain.Entities;

namespace Dilcore.Blueprints.Actors;

public static class EntityReferenceCommandExtensions
{
    public static EntityReferenceType GetInverseType(this AddEntityReferenceGrainCommand command) =>
        Enum.Parse<EntityReferenceType>(command.ReferenceType, ignoreCase: true).GetInverse();

    public static EntityReferenceType GetInverseType(this EntityReferenceGrainParameter parameter) =>
        Enum.Parse<EntityReferenceType>(parameter.ReferenceType, ignoreCase: true).GetInverse();

    public static AddEntityReferenceGrainCommand ToReverseCommand(
        this AddEntityReferenceGrainCommand command,
        string sourceSchemaName,
        Guid sourceEntityId,
        bool skipReverseReference = true)
    {
        var inverseType = command.GetInverseType();
        return new AddEntityReferenceGrainCommand
        {
            SchemaName = sourceSchemaName,
            ReferenceType = inverseType.ToString(),
            RelatedEntityDefinitionId = sourceEntityId,
            RelatedEntitySchemaName = sourceSchemaName,
            SkipReverseReference = skipReverseReference
        };
    }

    public static AddEntityReferenceGrainCommand ToReverseCommand(
        this EntityReferenceGrainParameter parameter,
        string sourceSchemaName,
        Guid sourceEntityId,
        bool skipReverseReference = true)
    {
        var inverseType = parameter.GetInverseType();
        return new AddEntityReferenceGrainCommand
        {
            SchemaName = sourceSchemaName,
            ReferenceType = inverseType.ToString(),
            RelatedEntityDefinitionId = sourceEntityId,
            RelatedEntitySchemaName = sourceSchemaName,
            SkipReverseReference = skipReverseReference
        };
    }

    public static AddEntityReferenceGrainCommand ToAddReferenceCommand(
        this EntityReferenceGrainParameter parameter,
        string relatedEntitySchemaName) =>
        new()
        {
            SchemaName = parameter.SchemaName,
            ReferenceType = parameter.ReferenceType,
            RelatedEntityDefinitionId = parameter.RelatedEntityDefinitionId,
            RelatedEntitySchemaName = relatedEntitySchemaName
        };

    public static EntityReferenceGrainDto ToEntityReferenceGrainDto(
        string schemaName,
        AddEntityReferenceGrainCommand command,
        string? reverseSchemaName = null) =>
        new()
        {
            SchemaName = schemaName,
            ReferenceType = command.ReferenceType,
            RelatedEntityDefinitionId = command.RelatedEntityDefinitionId,
            RelatedEntitySchemaName = command.RelatedEntitySchemaName,
            ReverseSchemaName = reverseSchemaName
        };
}
