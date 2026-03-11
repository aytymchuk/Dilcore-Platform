using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.MediatR.Abstractions;
using Dilcore.Results.Abstractions;
using FluentResults;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.RemoveReference;

public class RemoveEntityReferenceHandler : ICommandHandler<RemoveEntityReferenceCommand>
{
    private readonly IGrainFactory _grainFactory;

    public RemoveEntityReferenceHandler(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task<Result> Handle(
        RemoveEntityReferenceCommand request, CancellationToken cancellationToken)
    {
        var sourceGrain = _grainFactory.GetGrain<IEntityDefinitionGrain>(request.EntityDefinitionId);

        var result = await sourceGrain.RemoveReferenceAsync(new RemoveEntityReferenceGrainCommand { SchemaName = request.SchemaName });
        if (!result.IsSuccess)
            return Result.Fail(new NotFoundError(result.ErrorMessage ?? "Failed to remove reference."));

        return Result.Ok();
    }
}
