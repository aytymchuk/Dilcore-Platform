namespace Dilcore.Authentication.Abstractions;

/// <summary>
/// Provider for resolving user context from different sources.
/// </summary>
public interface IUserContextProvider
{
    /// <summary>
    /// Priority of this provider. Higher values = higher priority (checked first).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Attempts to resolve user context.
    /// Returns null if this provider cannot resolve the user.
    /// </summary>
    IUserContext? GetUserContext();
}