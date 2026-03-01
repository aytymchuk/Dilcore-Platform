using Dilcore.Blueprints.Domain.Entities;
using Dilcore.MediatR.Abstractions;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions.GetList;

public record GetEntityDefinitionsQuery : IQuery<IReadOnlyList<EntityDefinition>>;
