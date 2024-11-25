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
}
