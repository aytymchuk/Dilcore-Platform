namespace Dilcore.Blueprints.Actors.Abstractions;

[GenerateSerializer]
[Immutable]
public sealed record EntityDefinitionGrainResult
{
    public const string NotFoundCode = "NOT_FOUND";
    public const string ETagMismatchCode = "ETAG_MISMATCH";

    [Id(0)]
    public bool IsSuccess { get; init; }

    [Id(1)]
    public EntityDefinitionGrainDto? Entity { get; init; }

    [Id(2)]
    public string? ErrorMessage { get; init; }

    [Id(3)]
    public string? ErrorCode { get; init; }

    public static EntityDefinitionGrainResult Success(EntityDefinitionGrainDto entity) =>
        new() { IsSuccess = true, Entity = entity };

    public static EntityDefinitionGrainResult NotFound(string error) =>
        new() { IsSuccess = false, ErrorMessage = error, ErrorCode = NotFoundCode };

    public static EntityDefinitionGrainResult ETagMismatch(string error) =>
        new() { IsSuccess = false, ErrorMessage = error, ErrorCode = ETagMismatchCode };
}
