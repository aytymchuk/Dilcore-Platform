using Dilcore.Blueprints.Core.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.MediatR.Abstractions;
using FluentResults;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.GetList;

public class GetEntityDefinitionsHandler
    : IQueryHandler<GetEntityDefinitionsQuery, IReadOnlyList<EntityDefinition>>
{
    private readonly IEntityDefinitionRepository _repository;

    public GetEntityDefinitionsHandler(IEntityDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<EntityDefinition>>> Handle(
        GetEntityDefinitionsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}
