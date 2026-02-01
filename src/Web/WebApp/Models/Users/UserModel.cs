using Dilcore.Identity.Contracts.Profile;

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
    public string FullName =>($"{FirstName} {LastName}");

    /// <summary>
    /// Creates a new <see cref="UserModel"/> from a <see cref="UserDto"/>.
    /// </summary>
    /// <param name="dto">The DTO to map from.</param>
    /// <returns>The mapped user model.</returns>
    public static UserModel FromDto(UserDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new UserModel(
            dto.Id,
            dto.Email,
            dto.FirstName,
            dto.LastName);
    }
}
