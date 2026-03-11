using Dilcore.Blueprints.Domain.Entities;
using Dilcore.MediatR.Abstractions;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.AddReference;

public record AddEntityReferenceCommand : ICommand<EntityDefinition>
{
    public required Guid EntityDefinitionId { get; init; }
    public string? SchemaName { get; init; }
    public required EntityReferenceType ReferenceType { get; init; }
    public required Guid RelatedEntityDefinitionId { get; init; }
}
