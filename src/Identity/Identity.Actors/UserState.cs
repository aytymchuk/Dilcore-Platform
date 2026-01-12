namespace Dilcore.Identity.Actors;

/// <summary>
/// Serializable state for the UserGrain.
/// Stored in-memory grain storage.
/// </summary>
[GenerateSerializer]
public sealed class UserState
{
    [Id(0)]
    public string? Id { get; set; }

    [Id(1)]
    public string Email { get; set; } = string.Empty;

    [Id(2)]
    public string FullName { get; set; } = string.Empty;

    [Id(3)]
    public DateTime RegisteredAt { get; set; }

    [Id(4)]
    public bool IsRegistered { get; set; }
}
