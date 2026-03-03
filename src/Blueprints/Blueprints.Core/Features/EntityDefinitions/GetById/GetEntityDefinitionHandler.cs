using AutoMapper;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.MediatR.Abstractions;
using Dilcore.Results.Abstractions;
using FluentResults;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.GetById;

public class GetEntityDefinitionHandler : IQueryHandler<GetEntityDefinitionQuery, EntityDefinition>
{
    private readonly IGrainFactory _grainFactory;
    private readonly IMapper _mapper;

    public GetEntityDefinitionHandler(IGrainFactory grainFactory, IMapper mapper)
    {
        _grainFactory = grainFactory;
        _mapper = mapper;
    }

    public async Task<Result<EntityDefinition>> Handle(
        GetEntityDefinitionQuery request, CancellationToken cancellationToken)
    {
        var grain = _grainFactory.GetGrain<IEntityDefinitionGrain>(request.Id);
        var dto = await grain.GetAsync();

        if (dto is null)
            return Result.Fail<EntityDefinition>(new NotFoundError($"Entity definition '{request.Id}' not found."));

        return Result.Ok(_mapper.Map<EntityDefinition>(dto));
    }
}
