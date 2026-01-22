namespace Dilcore.Identity.Actors.Abstractions;

/// <summary>
/// Represents a user entity in the Orleans actor system.
/// Grain key is the user ID from IUserContext.Id.
/// </summary>
[Alias("Dilcore.Identity.Actors.Abstractions.IUserGrain")]
public interface IUserGrain : IGrainWithStringKey
{
    /// <summary>
    /// Registers a new user with the provided profile information.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <returns>The result of the registration operation.</returns>
    [Alias(nameof(RegisterAsync))]
    Task<UserCreationResult> RegisterAsync(string email, string firstName, string lastName);

    /// <summary>
    /// Gets the user's profile information.
    /// </summary>
    /// <returns>The user profile, or null if not registered.</returns>
    [Alias(nameof(GetProfileAsync))]
    Task<UserResponse?> GetProfileAsync();
}
