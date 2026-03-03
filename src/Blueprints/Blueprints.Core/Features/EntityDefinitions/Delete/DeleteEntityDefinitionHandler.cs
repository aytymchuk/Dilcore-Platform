using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.MediatR.Abstractions;
using Dilcore.Results.Abstractions;
using FluentResults;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.Delete;

public class DeleteEntityDefinitionHandler : ICommandHandler<DeleteEntityDefinitionCommand>
{
    private readonly IGrainFactory _grainFactory;

    public DeleteEntityDefinitionHandler(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task<Result> Handle(DeleteEntityDefinitionCommand request, CancellationToken cancellationToken)
    {
        var grain = _grainFactory.GetGrain<IEntityDefinitionGrain>(request.Id);
        var result = await grain.DeleteAsync();

        if (!result.IsSuccess)
            return Result.Fail(new NotFoundError(result.ErrorMessage ?? "Entity definition not found."));

        return Result.Ok();
    }
}
