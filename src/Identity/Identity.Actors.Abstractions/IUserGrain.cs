namespace Dilcore.Identity.Actors.Abstractions;

/// <summary>
/// Represents a user entity in the Orleans actor system.
/// Grain key is the user ID from IUserContext.Id.
/// </summary>
public interface IUserGrain : IGrainWithStringKey
{
    /// <summary>
    /// Registers a new user with the provided profile information.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="fullName">The user's full name.</param>
    /// <returns>The registered user profile.</returns>
    Task<UserDto> RegisterAsync(string email, string fullName);

    /// <summary>
    /// Gets the user's profile information.
    /// </summary>
    /// <returns>The user profile, or null if not registered.</returns>
    Task<UserDto?> GetProfileAsync();
}
