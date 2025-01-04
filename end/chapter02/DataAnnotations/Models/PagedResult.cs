namespace DataAnnotations.Models;

public class PagedResult<T>
{
    public IReadOnlyCollection<T>? Items { get; set; }

    public bool HasPreviousPage { get; set; }

    public bool HasNextPage { get; set; }

    public required string? PreviousPageUrl { get; set; }

    public required string? NextPageUrl { get; set; }

    public int PageSize { get; set; }
}
