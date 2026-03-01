using Dilcore.Blueprints.Contracts.EntityDefinitions;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Create;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Update;
using Dilcore.WebApi.Client.Clients;
using FluentResults;

namespace Dilcore.WebApi.Client.Extensions;

/// <summary>
/// Extension methods for <see cref="IBlueprintsClient"/> that provide Result-based error handling.
/// </summary>
public static class BlueprintsClientExtensions
{
    /// <summary>
    /// Safely gets all entity definitions, returning a Result instead of throwing exceptions.
    /// </summary>
    /// <param name="client">The blueprints client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the entity definition list or error information with ProblemDetails.</returns>
    public static Task<Result<IReadOnlyList<EntityDefinitionDto>>> SafeGetEntityDefinitionsAsync(
        this IBlueprintsClient client, CancellationToken ct = default)
        => SafeApiInvoker.InvokeAsync(() => client.GetEntityDefinitionsAsync(ct));

    /// <summary>
    /// Safely gets an entity definition by its identifier, returning a Result instead of throwing exceptions.
    /// </summary>
    /// <param name="client">The blueprints client.</param>
    /// <param name="id">The entity definition identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the entity definition or error information with ProblemDetails.</returns>
    public static Task<Result<EntityDefinitionDto>> SafeGetEntityDefinitionAsync(
        this IBlueprintsClient client, Guid id, CancellationToken ct = default)
        => SafeApiInvoker.InvokeAsync(() => client.GetEntityDefinitionAsync(id, ct));

    /// <summary>
    /// Safely creates a new entity definition, returning a Result instead of throwing exceptions.
    /// </summary>
    /// <param name="client">The blueprints client.</param>
    /// <param name="request">Entity definition creation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the created entity definition or error information with ProblemDetails.</returns>
    public static Task<Result<EntityDefinitionDto>> SafeCreateEntityDefinitionAsync(
        this IBlueprintsClient client, CreateEntityDefinitionDto request, CancellationToken ct = default)
        => SafeApiInvoker.InvokeAsync(() => client.CreateEntityDefinitionAsync(request, ct));

    /// <summary>
    /// Safely updates an existing entity definition, returning a Result instead of throwing exceptions.
    /// </summary>
    /// <param name="client">The blueprints client.</param>
    /// <param name="id">The entity definition identifier.</param>
    /// <param name="request">Entity definition update request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the updated entity definition or error information with ProblemDetails.</returns>
    public static Task<Result<EntityDefinitionDto>> SafeUpdateEntityDefinitionAsync(
        this IBlueprintsClient client, Guid id, UpdateEntityDefinitionDto request, CancellationToken ct = default)
        => SafeApiInvoker.InvokeAsync(() => client.UpdateEntityDefinitionAsync(id, request, ct));

    /// <summary>
    /// Safely deletes an entity definition by its identifier, returning a Result instead of throwing exceptions.
    /// </summary>
    /// <param name="client">The blueprints client.</param>
    /// <param name="id">The entity definition identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or error information with ProblemDetails.</returns>
    public static Task<Result> SafeDeleteEntityDefinitionAsync(
        this IBlueprintsClient client, Guid id, CancellationToken ct = default)
        => SafeApiInvoker.InvokeAsync(() => client.DeleteEntityDefinitionAsync(id, ct));
}
