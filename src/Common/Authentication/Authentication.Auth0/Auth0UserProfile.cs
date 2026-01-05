namespace Dilcore.Authentication.Auth0;

/// <summary>
/// Represents a user profile from Auth0.
/// </summary>
public sealed record Auth0UserProfile
{
    public string? Sub { get; init; }
    public string? Email { get; init; }
    public string? Name { get; init; }
    public string? GivenName { get; init; }
    public string? FamilyName { get; init; }
    public string? Nickname { get; init; }
    public string? Picture { get; init; }
}