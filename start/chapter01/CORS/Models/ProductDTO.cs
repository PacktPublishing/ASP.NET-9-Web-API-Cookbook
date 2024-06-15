namespace cookbook.Models;

public class ProductDTO
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int CategoryId { get; init; }
}
