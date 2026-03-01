using Dilcore.Blueprints.Domain.Entities;
using Dilcore.MediatR.Abstractions;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.GetList;

public record GetEntityDefinitionsQuery : IQuery<PagedEntityDefinitions>
{
    public int Skip { get; init; }
    public int Take { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public bool? IsAbstract { get; init; }
    public IReadOnlyList<string>? Tags { get; init; }
}
