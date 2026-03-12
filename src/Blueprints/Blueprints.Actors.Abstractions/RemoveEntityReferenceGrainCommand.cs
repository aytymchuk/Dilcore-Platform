namespace Dilcore.Blueprints.Actors.Abstractions;

[GenerateSerializer]
[Immutable]
public sealed record RemoveEntityReferenceGrainCommand
{
    [Id(0)]
    public required string SchemaName { get; init; }

    [Id(1)]
    public bool SkipReverseReference { get; init; }
}
