using Dilcore.Blueprints.Domain.Entities;
using Dilcore.MediatR.Abstractions;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.GetById;

public record GetEntityDefinitionQuery(Guid Id) : IQuery<EntityDefinition>;
