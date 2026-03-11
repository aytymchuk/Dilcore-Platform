using Dilcore.Blueprints.Domain;

namespace Dilcore.Blueprints.Actors;

public static class EntityReferenceSchemaNameExtensions
{
    public static string GenerateReferenceSchemaName(string sourceEntitySchemaName, string targetEntitySchemaName)
    {
        if (string.IsNullOrWhiteSpace(sourceEntitySchemaName))
        {
            return CompactSchemaName(targetEntitySchemaName);
        }

        if (string.IsNullOrWhiteSpace(targetEntitySchemaName))
        {
            return CompactSchemaName(sourceEntitySchemaName);
        }

        var sourceBase = CompactSchemaName(sourceEntitySchemaName);
        var targetBase = CompactSchemaName(targetEntitySchemaName);
        var targetEntityPascalCase = char.ToUpperInvariant(targetBase[0]) + targetBase[1..];
        var combined = $"{sourceBase}{targetEntityPascalCase}";

        string finalName;
        if (combined.Length <= EntityDefinitionLimits.SchemaNameMaxLength)
        {
            finalName = combined;
        }
        else
        {
            var maxSourceLength = Math.Max(1, EntityDefinitionLimits.SchemaNameMaxLength / 2);
            var truncatedSource = sourceBase[..Math.Min(sourceBase.Length, maxSourceLength)];
            var remainingForTarget = EntityDefinitionLimits.SchemaNameMaxLength - truncatedSource.Length;
            var truncatedTarget = targetEntityPascalCase[..Math.Min(targetEntityPascalCase.Length, remainingForTarget)];
            finalName = $"{truncatedSource}{truncatedTarget}";
        }

        return SchemaNameGenerator.Resolve(finalName, finalName);
    }

    public static string CompactSchemaName(string schemaName)
    {
        if (string.IsNullOrWhiteSpace(schemaName))
        {
            return "relation";
        }

        var firstDigitIndex = schemaName.IndexOfAny("0123456789".ToCharArray());
        var compact = firstDigitIndex > 0 ? schemaName[..firstDigitIndex] : schemaName;

        if (string.IsNullOrWhiteSpace(compact))
        {
            compact = "relation";
        }

        if (compact.Length > EntityDefinitionLimits.SchemaNameMaxLength)
        {
            compact = compact[..EntityDefinitionLimits.SchemaNameMaxLength];
        }

        return compact;
    }
}
