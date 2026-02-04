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
    private const string UserTenantsKey = "UserContext.Tenants";
    private const string UserRolesKey = "UserContext.Roles";

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

        if (userContext.Tenants.Any())
        {
            RequestContext.Set(UserTenantsKey, userContext.Tenants.ToArray());
        }
        else
        {
            RequestContext.Remove(UserTenantsKey);
        }

        if (userContext.Roles.Any())
        {
            RequestContext.Set(UserRolesKey, userContext.Roles.ToArray());
        }
        else
        {
            RequestContext.Remove(UserRolesKey);
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
        var tenants = RequestContext.Get(UserTenantsKey) as string[] ?? [];
        var roles = RequestContext.Get(UserRolesKey) as string[] ?? [];

        if (id is null && email is null && fullName is null && tenants.Length == 0 && roles.Length == 0)
        {
            return null;
        }

        return new UserContext(id ?? string.Empty, email, fullName, tenants, roles);
    }
}
