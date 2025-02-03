namespace Sales.Models;

public record OrderLine
{
    public int Id { get; init; }
    public int BookId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public int OrderId { get; init; }
    public required Order Order { get; init; }
}
