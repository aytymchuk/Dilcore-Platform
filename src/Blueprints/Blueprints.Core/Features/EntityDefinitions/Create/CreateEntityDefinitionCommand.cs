using Dilcore.Blueprints.Domain.Entities;
using Dilcore.MediatR.Abstractions;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.Create;

public record CreateEntityDefinitionCommand : ICommand<EntityDefinition>
{
    public required string DisplayName { get; init; }
    public string? Description { get; init; }
    public bool IsAbstract { get; init; }
    public Guid? ExtendsEntityId { get; init; }
    public IReadOnlyList<FieldDefinitionParameters> Fields { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}
