using FluentResults;

namespace Dilcore.WebApp.Features.Users.CurrentUser;

/// <summary>
/// Error indicating the current user was not found (not registered).
/// </summary>
public sealed class UserNotFoundError : Error
{
    public UserNotFoundError()
        : base("User not found. Registration required.")
    {
    }

    public UserNotFoundError(string message)
        : base(message)
    {
    }
}
