namespace Dilcore.Blueprints.Domain.Entities;

public record EntityMetadata
{
    public IReadOnlyList<string> Tags { get; init; } = [];
}
