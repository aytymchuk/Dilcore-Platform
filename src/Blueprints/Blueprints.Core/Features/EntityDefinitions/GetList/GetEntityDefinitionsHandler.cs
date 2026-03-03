using Dilcore.Blueprints.Core.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.MediatR.Abstractions;
using FluentResults;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.GetList;

public class GetEntityDefinitionsHandler
    : IQueryHandler<GetEntityDefinitionsQuery, PagedEntityDefinitions>
{
    private readonly IEntityDefinitionRepository _repository;

    public GetEntityDefinitionsHandler(IEntityDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedEntityDefinitions>> Handle(
        GetEntityDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetPagedAsync(
            request.Skip,
            request.Take,
            request.SearchTerm,
            request.IsAbstract,
            request.Tags,
            cancellationToken);

        if (result.IsFailed)
            return result.ToResult<PagedEntityDefinitions>();

        var (items, totalCount) = result.Value;
        return Result.Ok(new PagedEntityDefinitions(items, totalCount));
    }
}
