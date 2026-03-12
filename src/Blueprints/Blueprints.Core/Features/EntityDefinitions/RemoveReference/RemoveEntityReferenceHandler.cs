using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain;
using Dilcore.Blueprints.Core.Features.EntityDefinitions;
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
        var normalizedSchemaName = string.IsNullOrWhiteSpace(request.SchemaName)
            ? request.SchemaName ?? string.Empty
            : SchemaNameGenerator.Resolve(request.SchemaName, request.SchemaName);

        var result = await sourceGrain.RemoveReferenceAsync(new RemoveEntityReferenceGrainCommand { SchemaName = normalizedSchemaName });
        if (!result.IsSuccess)
        {
            var error = result.ToFluentError(
                "Entity definition not found.",
                "Validation failed.",
                "Failed to remove reference.");
            return Result.Fail(error);
        }

        return Result.Ok();
    }
}
