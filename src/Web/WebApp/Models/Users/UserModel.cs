namespace Dilcore.WebApp.Models.Users;

/// <summary>
/// Domain model representing the current user.
/// </summary>
public sealed record UserModel(
    Guid Id,
    string Email,
    string FirstName,
    string LastName)
{
    /// <summary>
    /// Full display name of the user.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}
