using Dilcore.Blueprints.Contracts;
using Dilcore.Blueprints.Contracts.EntityDefinitions;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Create;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Update;
using Refit;

namespace Dilcore.WebApi.Client.Clients;

/// <summary>
/// Refit client interface for Blueprints module endpoints.
/// </summary>
public interface IBlueprintsClient
{
    /// <summary>
    /// Gets a paginated list of entity definitions.
    /// </summary>
    [Get("/blueprints/entity-definitions")]
    Task<PagedResult<EntityDefinitionDto>> GetEntityDefinitionsAsync(
        [Query] int? skip = null,
        [Query] int? take = null,
        [Query] string? search = null,
        [Query] bool? isAbstract = null,
        [Query] string? tags = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets an entity definition by its identifier.
    /// </summary>
    /// <param name="id">The entity definition identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The entity definition details.</returns>
    [Get("/blueprints/entity-definitions/{id}")]
    Task<EntityDefinitionDto> GetEntityDefinitionAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new entity definition.
    /// </summary>
    /// <param name="request">Entity definition creation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created entity definition details.</returns>
    [Post("/blueprints/entity-definitions")]
    Task<EntityDefinitionDto> CreateEntityDefinitionAsync([Body] CreateEntityDefinitionDto request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing entity definition.
    /// </summary>
    /// <param name="id">The entity definition identifier.</param>
    /// <param name="request">Entity definition update request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated entity definition details.</returns>
    [Put("/blueprints/entity-definitions/{id}")]
    Task<EntityDefinitionDto> UpdateEntityDefinitionAsync(Guid id, [Body] UpdateEntityDefinitionDto request, CancellationToken ct = default);

    /// <summary>
    /// Deletes an entity definition by its identifier.
    /// </summary>
    /// <param name="id">The entity definition identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    [Delete("/blueprints/entity-definitions/{id}")]
    Task DeleteEntityDefinitionAsync(Guid id, CancellationToken ct = default);
}
