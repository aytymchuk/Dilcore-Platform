namespace Dilcore.Blueprints.Core.Features.EntityDefinitions;

public record FieldDefinitionParameters
{
    public required string DisplayName { get; init; }
    public required string Type { get; init; }
    public IReadOnlyList<FieldDefinitionParameters>? Fields { get; init; }
}
