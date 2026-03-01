using Dilcore.Blueprints.Domain.Entities;
using FluentResults;

namespace Dilcore.Blueprints.Core.Abstractions;

public interface IEntityDefinitionRepository
{
    Task<Result<EntityDefinition>> StoreAsync(EntityDefinition entity, CancellationToken cancellationToken = default);
    Task<Result<EntityDefinition?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<EntityDefinition>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<bool>> ExistsBySchemaNameAsync(string schemaName, CancellationToken cancellationToken = default);
}
