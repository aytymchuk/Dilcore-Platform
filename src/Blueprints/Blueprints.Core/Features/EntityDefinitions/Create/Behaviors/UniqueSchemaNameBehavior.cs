using Dilcore.Blueprints.Core.Abstractions;
using Dilcore.Blueprints.Domain;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Results.Abstractions;
using FluentResults;
using MediatR;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.Create.Behaviors;

public sealed class UniqueSchemaNameBehavior
    : IPipelineBehavior<CreateEntityDefinitionCommand, Result<EntityDefinition>>
{
    private readonly IEntityDefinitionRepository _repository;

    public UniqueSchemaNameBehavior(IEntityDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<EntityDefinition>> Handle(
        CreateEntityDefinitionCommand request,
        RequestHandlerDelegate<Result<EntityDefinition>> next,
        CancellationToken cancellationToken)
    {
        var schemaInput = !string.IsNullOrWhiteSpace(request.SchemaName)
            ? request.SchemaName
            : request.DisplayName;
        var schemaName = SchemaNameGenerator.Generate(schemaInput);

        var existsResult = await _repository.ExistsBySchemaNameAsync(schemaName, cancellationToken);
        if (existsResult.IsFailed)
            return Result.Fail<EntityDefinition>(existsResult.Errors);

        if (existsResult.Value)
            return Result.Fail<EntityDefinition>(
                new ConflictError($"An entity definition with schema name '{schemaName}' already exists."));

        return await next();
    }
}