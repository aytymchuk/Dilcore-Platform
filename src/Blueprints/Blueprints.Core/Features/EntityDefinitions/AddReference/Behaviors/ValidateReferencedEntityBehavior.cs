using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Results.Abstractions;
using FluentResults;
using MediatR;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.AddReference.Behaviors;

public sealed class ValidateReferencedEntityBehavior
    : IPipelineBehavior<AddEntityReferenceCommand, Result<EntityDefinition>>
{
    private readonly IGrainFactory _grainFactory;

    public ValidateReferencedEntityBehavior(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task<Result<EntityDefinition>> Handle(
        AddEntityReferenceCommand request,
        RequestHandlerDelegate<Result<EntityDefinition>> next,
        CancellationToken cancellationToken)
    {
        var grain = _grainFactory.GetGrain<IEntityDefinitionGrain>(request.RelatedEntityDefinitionId);
        var dto = await grain.GetAsync();

        if (dto is null)
            return Result.Fail<EntityDefinition>(
                new ValidationError($"Referenced entity definition '{request.RelatedEntityDefinitionId}' does not exist."));

        return await next(cancellationToken);
    }
}
