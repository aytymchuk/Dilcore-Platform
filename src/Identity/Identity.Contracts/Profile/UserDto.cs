namespace Dilcore.Identity.Contracts.Profile;

/// <summary>
/// API response contract for user profile information.
/// </summary>
/// <param name="Id">The unique user identifier.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="RegisteredAt">When the user was registered.</param>
/// <remarks>
/// This contract intentionally excludes IdentityId to avoid exposing internal identifiers.
/// </remarks>
public sealed record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime RegisteredAt);
