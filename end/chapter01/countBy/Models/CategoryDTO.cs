namespace cookbook.Models;

public record CategoryDTO
{
    public int CategoryId { get; init; }

    public int ProductCount { get; init; }
}