namespace Dilcore.Blueprints.Actors.Abstractions;

[GenerateSerializer]
[Immutable]
public sealed record EntityDefinitionGrainResult
{
    public const string NotFoundCode = "NOT_FOUND";
    public const string ETagMismatchCode = "ETAG_MISMATCH";
    public const string ValidationErrorCode = "VALIDATION_ERROR";

    [Id(0)]
    public bool IsSuccess { get; init; }

    [Id(1)]
    public EntityDefinitionGrainDto? Entity { get; init; }

    [Id(2)]
    public string? ErrorMessage { get; init; }

    [Id(3)]
    public string? ErrorCode { get; init; }

    [Id(4)]
    public IReadOnlyList<string>? AddedFields { get; init; }

    [Id(5)]
    public IReadOnlyList<string>? RemovedFields { get; init; }

    public static EntityDefinitionGrainResult Success(EntityDefinitionGrainDto entity) =>
        new() { IsSuccess = true, Entity = entity };

    public static EntityDefinitionGrainResult Success(
        EntityDefinitionGrainDto entity,
        IReadOnlyList<string> addedFields,
        IReadOnlyList<string> removedFields) =>
        new() { IsSuccess = true, Entity = entity, AddedFields = addedFields, RemovedFields = removedFields };

    public static EntityDefinitionGrainResult NotFound(string error) =>
        new() { IsSuccess = false, ErrorMessage = error, ErrorCode = NotFoundCode };

    public static EntityDefinitionGrainResult ETagMismatch(string error) =>
        new() { IsSuccess = false, ErrorMessage = error, ErrorCode = ETagMismatchCode };

    public static EntityDefinitionGrainResult Validation(string error) =>
        new() { IsSuccess = false, ErrorMessage = error, ErrorCode = ValidationErrorCode };
}
