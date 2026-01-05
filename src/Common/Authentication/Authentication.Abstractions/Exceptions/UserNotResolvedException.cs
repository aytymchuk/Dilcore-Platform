namespace Dilcore.Authentication.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when user context cannot be resolved from any registered provider.
/// </summary>
public class UserNotResolvedException : Exception
{
    public UserNotResolvedException()
    {
    }

    public UserNotResolvedException(string message) : base(message)
    {
    }

    public UserNotResolvedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}