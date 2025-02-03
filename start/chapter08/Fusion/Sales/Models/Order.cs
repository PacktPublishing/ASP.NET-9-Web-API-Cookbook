namespace Sales.Models;


public record Order
{
    public int Id { get; init; }
    public required string CustomerEmail { get; init; }
    public decimal Total { get; init; }
    public DateTime OrderDate { get; init; }
    public List<OrderLine> Lines { get; init; } = new();
}
