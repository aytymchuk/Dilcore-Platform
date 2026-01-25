using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Core.Features.Create;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApi.Client.Errors;
using FluentResults;
using Refit;

namespace Dilcore.WebApi.Client.Extensions;

/// <summary>
/// Extension methods for ITenancyClient that provide Result-based error handling.
/// </summary>
public static class TenancyClientExtensions
{
    /// <summary>
    /// Safely creates a new tenant, returning a Result instead of throwing exceptions.
    /// </summary>
    /// <param name="client">The tenancy client.</param>
    /// <param name="command">Tenant creation command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the created tenant details or error information with ProblemDetails.</returns>
    public static Task<Result<TenantDto>> SafeCreateTenantAsync(
        this ITenancyClient client,
        CreateTenantCommand command,
        CancellationToken cancellationToken = default)
    {
        return SafeApiInvoker.InvokeAsync(() => client.CreateTenantAsync(command, cancellationToken));
    }

    /// <summary>
    /// Safely gets the current tenant, returning a Result instead of throwing exceptions.
    /// </summary>
    /// <param name="client">The tenancy client.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the current tenant details or error information with ProblemDetails.</returns>
    public static Task<Result<TenantDto>> SafeGetTenantAsync(
        this ITenancyClient client,
        CancellationToken cancellationToken = default)
    {
        return SafeApiInvoker.InvokeAsync(() => client.GetTenantAsync(cancellationToken));
    }
}
