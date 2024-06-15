namespace cookbook.Models;

public abstract record PagedResponse<T>
{
	public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
	public int PageSize { get; init; }
	public bool HasPreviousPage { get; init; }
	public bool HasNextPage { get; init; }
}