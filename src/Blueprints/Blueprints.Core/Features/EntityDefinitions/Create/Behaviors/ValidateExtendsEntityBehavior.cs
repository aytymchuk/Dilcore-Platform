using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Results.Abstractions;
using FluentResults;
using MediatR;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.Create.Behaviors;

public sealed class ValidateExtendsEntityBehavior
    : IPipelineBehavior<CreateEntityDefinitionCommand, Result<EntityDefinition>>
{
    private readonly IGrainFactory _grainFactory;

    public ValidateExtendsEntityBehavior(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task<Result<EntityDefinition>> Handle(
        CreateEntityDefinitionCommand request,
        RequestHandlerDelegate<Result<EntityDefinition>> next,
        CancellationToken cancellationToken)
    {
        if (request.ExtendsEntityId is null)
            return await next(cancellationToken);

        var grain = _grainFactory.GetGrain<IEntityDefinitionGrain>(request.ExtendsEntityId.Value);
        var dto = await grain.GetAsync();

        if (dto is null)
            return Result.Fail<EntityDefinition>(
                new ValidationError($"Referenced entity definition '{request.ExtendsEntityId}' does not exist."));

        return await next(cancellationToken);
    }
}
