using Grpc.Core;
using InventoryService.Grpc;

namespace InventoryService.Services;

public class InventoryServiceImplementation(ILogger<InventoryServiceImplementation> logger) 
    : Inventory.InventoryBase
{
    public override Task<InitializeInventoryResponse> InitializeInventory(
        InitializeInventoryRequest request,
        ServerCallContext context)
    {
        logger.LogInformation(
            "Initializing inventory for Book {BookId} with ISBN {ISBN}, Initial Stock: {Stock}",
            request.BookId,
            request.Isbn,
            request.InitialStock);

        var inventoryId = Guid.NewGuid().ToString();
        
        logger.LogInformation(
            "Created inventory record {InventoryId} for Book {BookId}",
            inventoryId,
            request.BookId);

        return Task.FromResult(new InitializeInventoryResponse
        {
            Success = true,
            InventoryId = inventoryId
        });
    }

    public override async Task MonitorStock(
    IAsyncStreamReader<StockRequest> requestStream,
    IServerStreamWriter<StockUpdate> responseStream,
    ServerCallContext context)
    {
        await foreach (var stockRequest in requestStream.ReadAllAsync())
        {
            logger.LogInformation("Received stock request for Book ID: {BookId}", stockRequest.BookId);

            for (int i = 0; i < 5; i++)
            {
                var stockUpdate = new StockUpdate
                {
                    BookId = stockRequest.BookId,
                    CurrentStock = 100 - i * 10,
                    Location = "Warehouse A",
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                await responseStream.WriteAsync(stockUpdate);
                logger.LogInformation("Sent stock update: {Stock}", stockUpdate.CurrentStock);

                await Task.Delay(1000);
            }
        }
    }
}
