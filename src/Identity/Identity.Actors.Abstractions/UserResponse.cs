namespace Dilcore.Identity.Actors.Abstractions;

/// <summary>
/// Response contract for user profile information from the actor system.
/// </summary>
/// <param name="Id">The unique user identifier.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="RegisteredAt">When the user was registered.</param>
[GenerateSerializer]
public sealed record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime RegisteredAt);
