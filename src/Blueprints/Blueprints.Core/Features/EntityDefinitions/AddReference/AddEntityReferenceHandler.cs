using AutoMapper;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.MediatR.Abstractions;
using Dilcore.Results.Abstractions;
using FluentResults;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.AddReference;

public class AddEntityReferenceHandler
    : ICommandHandler<AddEntityReferenceCommand, EntityDefinition>
{
    private readonly IGrainFactory _grainFactory;
    private readonly IMapper _mapper;

    public AddEntityReferenceHandler(IGrainFactory grainFactory, IMapper mapper)
    {
        _grainFactory = grainFactory;
        _mapper = mapper;
    }

    public async Task<Result<EntityDefinition>> Handle(
        AddEntityReferenceCommand request, CancellationToken cancellationToken)
    {
        var sourceGrain = _grainFactory.GetGrain<IEntityDefinitionGrain>(request.EntityDefinitionId);
        var targetGrain = _grainFactory.GetGrain<IEntityDefinitionGrain>(request.RelatedEntityDefinitionId);

        // Existence is validated by ValidateReferencedEntityBehavior.
        var targetDto = await targetGrain.GetAsync();

        var grainCommand = new AddEntityReferenceGrainCommand
        {
            SchemaName = request.SchemaName,
            ReferenceType = request.ReferenceType.ToString(),
            RelatedEntityDefinitionId = request.RelatedEntityDefinitionId,
            RelatedEntitySchemaName = targetDto!.SchemaName
        };

        var result = await sourceGrain.AddReferenceAsync(grainCommand);
        if (!result.IsSuccess)
        {
            FluentResults.IError error = result.ErrorCode switch
            {
                EntityDefinitionGrainResult.NotFoundCode =>
                    new NotFoundError(result.ErrorMessage ?? "Entity definition not found."),
                EntityDefinitionGrainResult.ValidationErrorCode =>
                    new ValidationError(result.ErrorMessage ?? "Validation failed."),
                _ => new ValidationError(result.ErrorMessage ?? "Failed to add reference.")
            };
            return Result.Fail<EntityDefinition>(error);
        }

        var updatedDto = await sourceGrain.GetAsync();
        return Result.Ok(_mapper.Map<EntityDefinition>(updatedDto!));
    }
}
