namespace Dilcore.Blueprints.Contracts.EntityDefinitions;

public static class ValidationConstants
{
    public const int DisplayNameMinLength = 2;
    public const int DisplayNameMaxLength = 128;
    public const int DescriptionMaxLength = 200;

    public const int MaxTopLevelFields = 100;
    public const int MaxFieldsPerLevel = 50;
    public const int MaxNestingDepth = 5;

    public const int MaxTags = 20;
    public const int MaxTagLength = 64;

    public const int SchemaNameMaxLength = 64;

    public const string AlphanumericRequiredPattern = @"[a-zA-Z0-9]";
    public const string TagFormatPattern = @"^[a-zA-Z0-9_-]+$";
}
