using Dilcore.Identity.Contracts.Profile;
using Dilcore.Identity.Contracts.Register;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApi.Client.Errors;
using FluentResults;
using Refit;

namespace Dilcore.WebApi.Client.Extensions;

/// <summary>
/// Extension methods for IIdentityClient that provide Result-based error handling.
/// </summary>
public static class IdentityClientExtensions
{
    /// <summary>
    /// Safely registers a new user, returning a Result instead of throwing exceptions.
    /// </summary>
    /// <param name="client">The identity client.</param>
    /// <param name="dto">User registration data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the registered user details or error information with ProblemDetails.</returns>
    public static Task<Result<UserDto>> SafeRegisterUserAsync(
        this IIdentityClient client,
        RegisterUserDto dto,
        CancellationToken cancellationToken = default)
    {
        return SafeApiInvoker.InvokeAsync(() => client.RegisterUserAsync(dto, cancellationToken));
    }

    /// <summary>
    /// Safely gets the current authenticated user's profile, returning a Result instead of throwing exceptions.
    /// </summary>
    /// <param name="client">The identity client.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the current user's details or error information with ProblemDetails.</returns>
    public static Task<Result<UserDto>> SafeGetCurrentUserAsync(
        this IIdentityClient client,
        CancellationToken cancellationToken = default)
    {
        return SafeApiInvoker.InvokeAsync(() => client.GetCurrentUserAsync(cancellationToken));
    }
}
