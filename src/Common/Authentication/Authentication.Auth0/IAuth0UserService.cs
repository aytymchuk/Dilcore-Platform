namespace Dilcore.Authentication.Auth0;

/// <summary>
/// Service for interacting with Auth0 APIs to retrieve user information.
/// </summary>
public interface IAuth0UserService
{
    /// <summary>
    /// Retrieves user profile from Auth0 using the access token.
    /// </summary>
    /// <param name="accessToken">The access token from the current request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User profile if found, null otherwise.</returns>
    Task<Auth0UserProfile?> GetUserProfileAsync(string accessToken, CancellationToken cancellationToken = default);
}