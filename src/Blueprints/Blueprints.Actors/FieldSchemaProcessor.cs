using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain;

namespace Dilcore.Blueprints.Actors;

internal static class FieldSchemaProcessor
{
    public static List<FieldDefinitionGrainDto> GenerateSchemaNames(IReadOnlyList<FieldDefinitionGrainDto> fields) =>
        fields.Select(f => f with
        {
            SchemaName = SchemaNameGenerator.Generate(
                !string.IsNullOrEmpty(f.SchemaName) ? f.SchemaName : f.DisplayName),
            Fields = f.Fields is { Length: > 0 }
                ? GenerateSchemaNames(f.Fields).ToArray()
                : f.Fields
        }).ToList();

    /// <summary>
    /// Merges incoming fields with existing fields. Existing fields (matched by SchemaName)
    /// preserve their SchemaName. New fields get SchemaNames generated from DisplayName.
    /// </summary>
    public static List<FieldDefinitionGrainDto> MergeWithExisting(
        IReadOnlyList<FieldDefinitionGrainDto> incoming,
        IReadOnlyList<FieldDefinitionGrainDto> existing)
    {
        var existingBySchema = existing
            .GroupBy(f => f.SchemaName, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        return incoming.Select(f => MergeField(f, existingBySchema)).ToList();
    }

    public static string? FindDuplicate(IReadOnlyList<FieldDefinitionGrainDto> fields) =>
        FindDuplicate(fields, new HashSet<string>(StringComparer.Ordinal));

    private static string? FindDuplicate(IReadOnlyList<FieldDefinitionGrainDto> fields, HashSet<string> seen)
    {
        foreach (var field in fields)
        {
            if (!seen.Add(field.SchemaName))
                return field.SchemaName;

            if (field.Fields is { Length: > 0 })
            {
                var nested = FindDuplicate(field.Fields, seen);
                if (nested is not null)
                    return nested;
            }
        }

        return null;
    }

    public static string? FindReserved(IReadOnlyList<FieldDefinitionGrainDto> fields)
    {
        foreach (var field in fields)
        {
            if (SchemaNameGenerator.IsReserved(field.SchemaName))
                return field.SchemaName;

            if (field.Fields is { Length: > 0 })
            {
                var nested = FindReserved(field.Fields);
                if (nested is not null)
                    return nested;
            }
        }

        return null;
    }

    /// <summary>
    /// Validates field schema name integrity: checks for duplicates and reserved names in one pass.
    /// Returns a validation error message or <c>null</c> if all names are valid.
    /// </summary>
    public static string? ValidateSchemaNames(IReadOnlyList<FieldDefinitionGrainDto> fields)
    {
        var duplicate = FindDuplicate(fields);
        if (duplicate is not null)
            return $"Duplicate field schema name '{duplicate}'.";

        var reserved = FindReserved(fields);
        if (reserved is not null)
            return $"Field schema name '{reserved}' is reserved.";

        return null;
    }

    public static FieldChanges ComputeChanges(
        IReadOnlyList<FieldDefinitionGrainDto> oldFields,
        IReadOnlyList<FieldDefinitionGrainDto> newFields)
    {
        var oldNames = CollectSchemaNames(oldFields);
        var newNames = CollectSchemaNames(newFields);

        return new FieldChanges(
            Added: newNames.Except(oldNames).ToList(),
            Removed: oldNames.Except(newNames).ToList());
    }

    private static HashSet<string> CollectSchemaNames(IReadOnlyList<FieldDefinitionGrainDto> fields)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        CollectSchemaNamesRecursive(fields, result);
        return result;
    }

    private static void CollectSchemaNamesRecursive(
        IReadOnlyList<FieldDefinitionGrainDto> fields, HashSet<string> result)
    {
        foreach (var field in fields)
        {
            result.Add(field.SchemaName);
            if (field.Fields is { Length: > 0 })
                CollectSchemaNamesRecursive(field.Fields, result);
        }
    }

    private static FieldDefinitionGrainDto MergeField(
        FieldDefinitionGrainDto field,
        Dictionary<string, FieldDefinitionGrainDto> existingBySchema)
    {
        var schemaName = ResolveSchemaName(field, existingBySchema);
        var nestedExisting = GetNestedExisting(schemaName, existingBySchema);

        return field with
        {
            SchemaName = schemaName,
            Fields = field.Fields is { Length: > 0 }
                ? MergeWithExisting(field.Fields, nestedExisting).ToArray()
                : field.Fields
        };
    }

    private static string ResolveSchemaName(
        FieldDefinitionGrainDto field,
        Dictionary<string, FieldDefinitionGrainDto> existingBySchema) =>
        !string.IsNullOrEmpty(field.SchemaName) && existingBySchema.ContainsKey(field.SchemaName)
            ? field.SchemaName
            : SchemaNameGenerator.Generate(field.DisplayName);

    private static FieldDefinitionGrainDto[] GetNestedExisting(
        string schemaName,
        Dictionary<string, FieldDefinitionGrainDto> existingBySchema) =>
        existingBySchema.TryGetValue(schemaName, out var match) && match.Fields is not null
            ? match.Fields
            : [];
}
