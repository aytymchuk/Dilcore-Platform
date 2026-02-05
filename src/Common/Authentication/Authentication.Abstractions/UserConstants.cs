namespace Dilcore.Authentication.Abstractions;

/// <summary>
/// Constants for user-related claim types and configuration.
/// </summary>
public static class UserConstants
{
    /// <summary>
    /// Standard claim type for user identifier (subject).
    /// </summary>
    public const string SubjectClaimType = "sub";

    /// <summary>
    /// Reserved identifier for system-level operations.
    /// </summary>
    public const string SystemUserId = "system";

    /// <summary>
    /// Alternative claim type for user identifier.
    /// </summary>
    public const string UserIdClaimType = "uid";

    /// <summary>
    /// Claim type for user email address.
    /// </summary>
    public const string EmailClaimType = "email";

    /// <summary>
    /// Claim type for user's full name.
    /// </summary>
    public const string NameClaimType = "name";

    /// <summary>
    /// Claim type for user's tenants.
    /// </summary>
    public const string TenantsClaimType = "tenants";

    /// <summary>
    /// Claim type for user's roles.
    /// </summary>
    [Obsolete("Use ClaimTypes.Role for standard role claims. RolesClaimType is for external providers like Auth0 that use 'roles' string.")]
    public const string RolesClaimType = "roles";
}