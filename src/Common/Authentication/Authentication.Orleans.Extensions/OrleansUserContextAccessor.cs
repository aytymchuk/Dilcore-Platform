using Dilcore.Authentication.Abstractions;

namespace Dilcore.Authentication.Orleans.Extensions;

/// <summary>
/// Provides access to user context stored in Orleans RequestContext.
/// This allows user information to flow across grain calls.
/// </summary>
public static class OrleansUserContextAccessor
{
    private const string UserIdKey = "UserContext.Id";
    private const string UserEmailKey = "UserContext.Email";
    private const string UserFullNameKey = "UserContext.FullName";

    /// <summary>
    /// Sets the user context in Orleans RequestContext.
    /// </summary>
    /// <param name="userContext">The user context to store.</param>
    public static void SetUserContext(IUserContext? userContext)
    {
        if (userContext is null)
        {
            RequestContext.Remove(UserIdKey);
            RequestContext.Remove(UserEmailKey);
            RequestContext.Remove(UserFullNameKey);
            return;
        }

        if (userContext.Id is not null)
        {
            RequestContext.Set(UserIdKey, userContext.Id);
        }
        else
        {
            RequestContext.Remove(UserIdKey);
        }

        if (userContext.Email is not null)
        {
            RequestContext.Set(UserEmailKey, userContext.Email);
        }
        else
        {
            RequestContext.Remove(UserEmailKey);
        }

        if (userContext.FullName is not null)
        {
            RequestContext.Set(UserFullNameKey, userContext.FullName);
        }
        else
        {
            RequestContext.Remove(UserFullNameKey);
        }
    }

    /// <summary>
    /// Gets the user context from Orleans RequestContext.
    /// </summary>
    /// <returns>The user context if available, otherwise null.</returns>
    public static IUserContext? GetUserContext()
    {
        var id = RequestContext.Get(UserIdKey) as string;
        var email = RequestContext.Get(UserEmailKey) as string;
        var fullName = RequestContext.Get(UserFullNameKey) as string;

        if (id is null && email is null && fullName is null)
        {
            return null;
        }

        return new UserContext(id, email, fullName);
    }
}
