using AutoMapper;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Blueprints.Core.Features.EntityDefinitions;
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
            var error = result.ToFluentError(
                "Entity definition not found.",
                "Validation failed.",
                "Failed to add reference.");
            return Result.Fail<EntityDefinition>(error);
        }

        if (result.Entity is null)
        {
            return Result.Fail<EntityDefinition>(
                new UnexpectedError("Grain returned success without entity for added reference."));
        }

        return Result.Ok(_mapper.Map<EntityDefinition>(result.Entity));
    }
}
