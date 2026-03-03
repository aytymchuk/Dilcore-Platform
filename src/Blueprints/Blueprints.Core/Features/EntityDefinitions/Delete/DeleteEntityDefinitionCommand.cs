using Dilcore.MediatR.Abstractions;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.Delete;

public record DeleteEntityDefinitionCommand(Guid Id) : ICommand;
