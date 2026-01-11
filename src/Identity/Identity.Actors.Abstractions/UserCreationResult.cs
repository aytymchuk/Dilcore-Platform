using Dilcore.Identity.Actors.Abstractions;

namespace Dilcore.Identity.Actors.Abstractions;

/// <summary>
/// Result of a user creation operation.
/// Designed to be Orleans-serializable.
/// </summary>
[GenerateSerializer]
public sealed record UserCreationResult
{
    [Id(0)]
    public bool IsSuccess { get; init; }

    [Id(1)]
    public UserDto? User { get; init; }

    [Id(2)]
    public string? ErrorMessage { get; init; }

    public static UserCreationResult Success(UserDto user) => new() { IsSuccess = true, User = user };
    public static UserCreationResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}
