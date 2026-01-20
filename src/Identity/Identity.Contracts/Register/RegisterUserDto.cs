namespace Dilcore.Identity.Contracts.Register;

/// <summary>
/// Request contract for user registration.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
public sealed record RegisterUserDto(string Email, string FirstName, string LastName);
