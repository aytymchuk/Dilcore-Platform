namespace Dilcore.Common.Domain.Abstractions;

public abstract record BaseDomain
{
    public Guid Id { get; init; }
    public long ETag { get; init; }
    public DateTime CreatedOn { get; init; }
    public DateTime? UpdatedOn { get; init; }
}