namespace Dilcore.Blueprints.Contracts;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public long TotalCount { get; set; }
}
