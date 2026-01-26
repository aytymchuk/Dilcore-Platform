using Dilcore.Identity.Contracts.Profile;
using Dilcore.Identity.Contracts.Register;
using Refit;

namespace Dilcore.WebApi.Client.Clients;

/// <summary>
/// Refit client interface for Identity module endpoints.
/// </summary>
public interface IIdentityClient
{
    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="dto">User registration data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The registered user details.</returns>
    [Post("/users/register")]
    Task<UserDto> RegisterUserAsync([Body] RegisterUserDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current authenticated user's profile.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current user's details.</returns>
    [Get("/users/me")]
    Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
