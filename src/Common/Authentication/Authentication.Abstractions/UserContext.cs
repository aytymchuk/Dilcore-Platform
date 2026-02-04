namespace Dilcore.Authentication.Abstractions;

/// <summary>
/// Immutable record representing resolved user context.
/// </summary>
public sealed record UserContext(
    string Id,
    string? Email,
    string? FullName,
    IEnumerable<string> Tenants,
    IEnumerable<string> Roles) : IUserContext
{
    /// <summary>
    /// Represents a null/empty user context (no user resolved).
    /// </summary>
    public static readonly UserContext Empty = new(string.Empty, null, null, [], []);
}