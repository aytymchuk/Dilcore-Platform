using Dilcore.Blueprints.Domain.Entities.Fields;

namespace Dilcore.Blueprints.Actors.Abstractions;

public static class FieldDefinitionMapper
{
    public static FieldDefinition ToFieldDefinition(FieldDefinitionGrainDto dto) =>
        IsComplexType(dto.Type)
            ? new ComplexFieldDefinition
            {
                SchemaName = dto.SchemaName,
                DisplayName = dto.DisplayName,
                Type = Enum.Parse<FieldType>(dto.Type, ignoreCase: true),
                Fields = (dto.Fields ?? []).Select(ToFieldDefinition).ToList()
            }
            : new FieldDefinition
            {
                SchemaName = dto.SchemaName,
                DisplayName = dto.DisplayName,
                Type = Enum.Parse<FieldType>(dto.Type, ignoreCase: true)
            };

    public static bool IsComplexType(string type) =>
        type is nameof(FieldType.Object) or nameof(FieldType.Array);
}
