using Dilcore.Blueprints.Domain.Entities;
using Dilcore.MediatR.Abstractions;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.Update;

public record UpdateEntityDefinitionCommand : ICommand<EntityDefinition>
{
    public required Guid Id { get; init; }
    public long ETag { get; init; }
    public string? Description { get; init; }
    public bool IsAbstract { get; init; }
    public Guid? ExtendsEntityId { get; init; }
    public IReadOnlyList<FieldDefinitionParameters> Fields { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}
