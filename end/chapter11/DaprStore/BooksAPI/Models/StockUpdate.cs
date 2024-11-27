namespace Books.Models;

public record StockUpdate(
    int BookId,
    int CurrentStock,
    string Location,
    long Timestamp);
