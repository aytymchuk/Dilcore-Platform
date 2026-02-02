namespace Dilcore.Tenancy.Actors;

/// <summary>
/// Serializable state for the TenantGrain.
/// Stored in-memory grain storage.
/// </summary>
[GenerateSerializer]
public sealed class TenantState
{
    [Id(0)]
    public Guid Id { get; set; }

    [Id(1)]
    public string SystemName { get; set; } = string.Empty;

    [Id(2)]
    public string Name { get; set; } = string.Empty;

    [Id(3)]
    public string? Description { get; set; } = string.Empty;

    [Id(4)]
    public DateTime CreatedAt { get; set; }

    [Id(5)]
    public bool IsCreated { get; set; }

    [Id(6)]
    public string StoragePrefix { get; set; } = string.Empty;
}
