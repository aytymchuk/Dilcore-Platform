using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain;
using Dilcore.Blueprints.Domain.Entities.Fields;

namespace Dilcore.Blueprints.Actors;

/// <summary>
/// Validates the processed field tree after schema name generation/merging.
/// Input-level validation (display names, description, tags) is handled by Contracts validators.
/// </summary>
internal static class EntityDefinitionValidator
{
    private static readonly HashSet<string> ValidFieldTypes =
        Enum.GetNames<FieldType>().ToHashSet(StringComparer.Ordinal);

    private static readonly HashSet<string> ComplexFieldTypes = new(StringComparer.Ordinal)
    {
        nameof(FieldType.Object), nameof(FieldType.Array)
    };

    public static string? ValidateFields(List<FieldDefinitionGrainDto> fields) =>
        ValidateFieldsRecursive(fields, EntityDefinitionLimits.MaxTopLevelFields, depth: 1);

    private static string? ValidateFieldsRecursive(List<FieldDefinitionGrainDto> fields, int maxCount, int depth)
    {
        if (fields.Count > maxCount)
            return $"Must not have more than {maxCount} fields at depth {depth}.";

        foreach (var field in fields)
        {
            var error = ValidateFieldStructure(field, depth);
            if (error is not null)
                return error;
        }

        return null;
    }

    private static string? ValidateFieldStructure(FieldDefinitionGrainDto field, int depth)
    {
        if (!ValidFieldTypes.Contains(field.Type))
            return $"Field type '{field.Type}' is not valid.";

        var isComplex = ComplexFieldTypes.Contains(field.Type);

        if (isComplex && field.Fields is not { Count: > 0 })
            return $"Field '{field.DisplayName}' of type {field.Type} must contain at least one nested field.";

        if (!isComplex && field.Fields is { Count: > 0 })
            return $"Field '{field.DisplayName}' of type {field.Type} must not have nested fields.";

        if (field.Fields is { Count: > 0 })
        {
            if (depth >= EntityDefinitionLimits.MaxNestingDepth)
                return $"Field nesting depth must not exceed {EntityDefinitionLimits.MaxNestingDepth} levels.";

            return ValidateFieldsRecursive(field.Fields, EntityDefinitionLimits.MaxFieldsPerLevel, depth + 1);
        }

        return null;
    }
}
