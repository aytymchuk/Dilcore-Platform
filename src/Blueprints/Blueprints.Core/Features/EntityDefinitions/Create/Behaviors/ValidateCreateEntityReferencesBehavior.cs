using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Results.Abstractions;
using FluentResults;
using MediatR;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.Create.Behaviors;

public sealed class ValidateCreateEntityReferencesBehavior
    : IPipelineBehavior<CreateEntityDefinitionCommand, Result<EntityDefinition>>
{
    private readonly IGrainFactory _grainFactory;

    public ValidateCreateEntityReferencesBehavior(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task<Result<EntityDefinition>> Handle(
        CreateEntityDefinitionCommand request,
        RequestHandlerDelegate<Result<EntityDefinition>> next,
        CancellationToken cancellationToken)
    {
        if (request.References is null || !request.References.Any())
            return await next(cancellationToken);

        var tasks = request.References.Select(reference =>
            _grainFactory.GetGrain<IEntityDefinitionGrain>(reference.RelatedEntityDefinitionId).GetAsync()).ToList();
        var results = await Task.WhenAll(tasks);

        for (var i = 0; i < results.Length; i++)
        {
            if (results[i] is null)
            {
                return Result.Fail<EntityDefinition>(
                    new ValidationError($"Referenced entity definition '{request.References[i].RelatedEntityDefinitionId}' does not exist."));
            }
        }

        return await next(cancellationToken);
    }
}