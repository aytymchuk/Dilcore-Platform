using System.Text.RegularExpressions;

namespace Dilcore.Blueprints.Domain;

public static partial class EntityDefinitionLimits
{
    public const int DisplayNameMinLength = 2;
    public const int DisplayNameMaxLength = 128;
    public const int DescriptionMaxLength = 200;

    public const int MaxTopLevelFields = 100;
    public const int MaxFieldsPerLevel = 50;
    public const int MaxNestingDepth = 5;

    public const int MaxTags = 20;
    public const int MaxTagLength = 64;

    [GeneratedRegex(@"[a-zA-Z0-9]")]
    public static partial Regex AlphanumericRequiredRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9_-]+$")]
    public static partial Regex TagFormatRegex();
}
