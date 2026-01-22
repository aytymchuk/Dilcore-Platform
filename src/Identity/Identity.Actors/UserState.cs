namespace Dilcore.Identity.Actors;

/// <summary>
/// Serializable state for the UserGrain.
/// Stored in-memory grain storage.
/// </summary>
[GenerateSerializer]
public sealed class UserState
{
    [Id(0)]
    public string? IdentityId { get; set; }

    [Id(7)]
    public Guid Id { get; set; }

    [Id(1)]
    public string Email { get; set; } = string.Empty;

    [Id(2)]
    public string FirstName { get; set; } = string.Empty;

    [Id(3)]
    public string LastName { get; set; } = string.Empty;

    [Id(4)]
    public DateTime RegisteredAt { get; set; }

    [Id(5)]
    public bool IsRegistered { get; set; }

    [Id(6)]
    public long ETag { get; set; }

    [Id(8)]
    public DateTime? UpdatedAt { get; set; }
}
