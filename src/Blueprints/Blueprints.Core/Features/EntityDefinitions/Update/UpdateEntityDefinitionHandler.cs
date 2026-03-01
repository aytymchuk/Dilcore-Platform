using AutoMapper;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.MediatR.Abstractions;
using Dilcore.Results.Abstractions;
using FluentResults;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.Update;

public class UpdateEntityDefinitionHandler
    : ICommandHandler<UpdateEntityDefinitionCommand, EntityDefinition>
{
    private readonly IGrainFactory _grainFactory;
    private readonly IMapper _mapper;

    public UpdateEntityDefinitionHandler(IGrainFactory grainFactory, IMapper mapper)
    {
        _grainFactory = grainFactory;
        _mapper = mapper;
    }

    public async Task<Result<EntityDefinition>> Handle(
        UpdateEntityDefinitionCommand request, CancellationToken cancellationToken)
    {
        var grain = _grainFactory.GetGrain<IEntityDefinitionGrain>(request.Id);

        var grainCommand = _mapper.Map<UpdateEntityDefinitionGrainCommand>(request);
        var result = await grain.UpdateAsync(grainCommand);

        if (!result.IsSuccess)
        {
            var error = result.ErrorCode == EntityDefinitionGrainResult.ETagMismatchCode
                ? (FluentResults.IError)new ConflictError(result.ErrorMessage ?? "ETag mismatch.")
                : new NotFoundError(result.ErrorMessage ?? "Entity definition not found.");

            return Result.Fail<EntityDefinition>(error);
        }

        return Result.Ok(_mapper.Map<EntityDefinition>(result.Entity!));
    }
}
