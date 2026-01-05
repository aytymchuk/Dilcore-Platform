namespace Dilcore.Authentication.Abstractions;

/// <summary>
/// Resolves user context by trying registered providers in priority order.
/// </summary>
public interface IUserContextResolver
{
    /// <summary>
    /// Resolves the current user context.
    /// </summary>
    /// <returns>The resolved user context.</returns>
    /// <exception cref="Exceptions.UserNotResolvedException">Thrown when user context cannot be resolved.</exception>
    IUserContext Resolve();

    /// <summary>
    /// Attempts to resolve the user context.
    /// </summary>
    /// <param name="userContext">The resolved user context, or null if resolution failed.</param>
    /// <returns>True if user context was successfully resolved; otherwise, false.</returns>
    bool TryResolve(out IUserContext? userContext);
}