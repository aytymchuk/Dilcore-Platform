namespace Dilcore.Tenancy.Actors;

/// <summary>
/// Serializable state for the TenantGrain.
/// Stored in-memory grain storage.
/// </summary>
[GenerateSerializer]
public sealed class TenantState
{
    [Id(0)]
    public string Name { get; set; } = string.Empty;

    [Id(1)]
    public string DisplayName { get; set; } = string.Empty;

    [Id(2)]
    public string Description { get; set; } = string.Empty;

    [Id(3)]
    public DateTime CreatedAt { get; set; }

    [Id(4)]
    public bool IsCreated { get; set; }
}
