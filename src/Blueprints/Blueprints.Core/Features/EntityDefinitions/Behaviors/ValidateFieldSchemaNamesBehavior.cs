using Dilcore.Blueprints.Domain;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Results.Abstractions;
using FluentResults;
using MediatR;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.Behaviors;

public sealed class ValidateFieldSchemaNamesBehavior<TRequest>
    : IPipelineBehavior<TRequest, Result<EntityDefinition>>
    where TRequest : IEntityDefinitionFieldsCommand
{
    public Task<Result<EntityDefinition>> Handle(
        TRequest request,
        RequestHandlerDelegate<Result<EntityDefinition>> next,
        CancellationToken cancellationToken)
    {
        var invalidName = FindFirstInvalidSchemaName(request.Fields);

        if (invalidName is not null)
            return Task.FromResult(
                Result.Fail<EntityDefinition>(
                    new ValidationError(
                        $"Field schema name '{invalidName}' cannot be normalized to a valid camelCase schema name.")));

        return next(cancellationToken);
    }

    private static string? FindFirstInvalidSchemaName(IReadOnlyList<FieldDefinitionParameters> fields)
    {
        foreach (var field in fields)
        {
            if (!string.IsNullOrWhiteSpace(field.SchemaName) && !IsValidOrNormalizable(field.SchemaName))
                return field.SchemaName;

            if (field.Fields is { Count: > 0 })
            {
                var nested = FindFirstInvalidSchemaName(field.Fields);
                if (nested is not null)
                    return nested;
            }
        }

        return null;
    }

    private static bool IsValidOrNormalizable(string schemaName)
    {
        if (SchemaNameGenerator.IsValid(schemaName))
            return true;

        var normalized = SchemaNameGenerator.Generate(schemaName);
        return SchemaNameGenerator.IsValid(normalized);
    }
}
