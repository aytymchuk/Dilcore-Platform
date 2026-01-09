namespace Dilcore.Identity.Actors.Abstractions;

/// <summary>
/// Data transfer object for user profile information.
/// </summary>
/// <param name="Id">The unique user identifier.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="FullName">The user's full name.</param>
/// <param name="RegisteredAt">When the user was registered.</param>
[GenerateSerializer]
public sealed record UserDto(
    string Id,
    string Email,
    string FullName,
    DateTime RegisteredAt);
