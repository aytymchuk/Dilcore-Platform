using Dilcore.MediatR.Abstractions;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.RemoveReference;

public record RemoveEntityReferenceCommand(Guid EntityDefinitionId, string SchemaName) : ICommand;
