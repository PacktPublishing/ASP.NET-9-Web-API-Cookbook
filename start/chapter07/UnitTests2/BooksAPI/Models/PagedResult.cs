namespace books.Models;

public class PagedResult<T>
{
    public IReadOnlyCollection<T>? Items { get; set; }

    public bool HasPreviousPage { get; set; }

    public bool HasNextPage { get; set; }

    public string? PreviousPageUrl { get; set; } = string.Empty;

    public string? NextPageUrl { get; set; } = string.Empty;

    public int PageSize { get; set; }
}
