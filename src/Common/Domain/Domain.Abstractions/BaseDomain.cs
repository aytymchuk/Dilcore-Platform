namespace Dilcore.Domain.Abstractions;

public abstract record BaseDomain
{
    public Guid Id { get; init; }
    public long ETag { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}