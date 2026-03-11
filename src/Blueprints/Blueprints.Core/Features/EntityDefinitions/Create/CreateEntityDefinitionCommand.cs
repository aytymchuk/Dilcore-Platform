using Dilcore.Blueprints.Domain.Entities;
using Dilcore.MediatR.Abstractions;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.Create;

public record CreateEntityDefinitionCommand : ICommand<EntityDefinition>, IEntityDefinitionFieldsCommand
{
    public string? SchemaName { get; init; }
    public required string DisplayName { get; init; }
    public string? Description { get; init; }
    public bool IsAbstract { get; init; }
    public Guid? ExtendsEntityId { get; init; }
    public IReadOnlyList<FieldDefinitionParameters> Fields { get; init; } = [];
    public IReadOnlyList<EntityReferenceParameters> References { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public record EntityReferenceParameters
{
    public string? SchemaName { get; init; }
    public required EntityReferenceType ReferenceType { get; init; }
    public required Guid RelatedEntityDefinitionId { get; init; }
}
