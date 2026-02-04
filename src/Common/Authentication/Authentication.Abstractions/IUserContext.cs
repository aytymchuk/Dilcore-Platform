namespace Dilcore.Authentication.Abstractions;

/// <summary>
/// Abstraction for accessing current user information across different contexts.
/// Used for authentication, authorization, and telemetry.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// The unique identifier for the user (from JWT 'sub' or 'uid' claim).
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// The user's full name.
    /// </summary>
    string? FullName { get; }

    /// <summary>
    /// The list of tenant IDs the user belongs to.
    /// </summary>
    IEnumerable<string> Tenants { get; }

    /// <summary>
    /// The list of roles for the CURRENT tenant context.
    /// </summary>
    IEnumerable<string> Roles { get; }
}