using AutoMapper;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.MediatR.Abstractions;
using Dilcore.Results.Abstractions;
using FluentResults;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.Create;

public class CreateEntityDefinitionHandler
    : ICommandHandler<CreateEntityDefinitionCommand, EntityDefinition>
{
    private readonly IGrainFactory _grainFactory;
    private readonly IMapper _mapper;

    public CreateEntityDefinitionHandler(IGrainFactory grainFactory, IMapper mapper)
    {
        _grainFactory = grainFactory;
        _mapper = mapper;
    }

    public async Task<Result<EntityDefinition>> Handle(
        CreateEntityDefinitionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return Result.Fail<EntityDefinition>(new ValidationError("DisplayName is required."));

        var grainId = Guid.CreateVersion7();
        var grain = _grainFactory.GetGrain<IEntityDefinitionGrain>(grainId);

        var grainCommand = _mapper.Map<CreateEntityDefinitionGrainCommand>(request);
        var result = await grain.CreateAsync(grainCommand);

        if (!result.IsSuccess)
        {
            FluentResults.IError error = result.ErrorCode switch
            {
                EntityDefinitionGrainResult.ValidationErrorCode =>
                    new ValidationError(result.ErrorMessage ?? "Validation failed."),
                EntityDefinitionGrainResult.AlreadyExistsCode =>
                    new ConflictError(result.ErrorMessage ?? "Entity definition already exists."),
                _ => new ConflictError(result.ErrorMessage ?? "Failed to create entity definition.")
            };

            return Result.Fail<EntityDefinition>(error);
        }

        return Result.Ok(_mapper.Map<EntityDefinition>(result.Entity!));
    }
}
