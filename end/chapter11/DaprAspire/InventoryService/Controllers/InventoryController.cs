using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using InventoryService.Models;

namespace InventoryService.Controllers;

[ApiController]
[Route("[controller]")]
public class InventoryController(
    DaprClient daprClient,
    ILogger<InventoryController> _logger) : ControllerBase
    {
    [HttpPost("monitor/{bookId}")]
public async Task<IActionResult> PublishStockUpdates(int bookId)
{
    var scenarios = new[]
    {
        (stock: 100, location: "Main Warehouse"),
        (stock: 75, location: "Store Front"),
        (stock: 50, location: "Online Fulfillment"),
        (stock: 25, location: "Reserve Stock"),
        (stock: 10, location: "Last Units Warning")
    };

    foreach (var scenario in scenarios)
    {
        var update = new StockUpdate(
            BookId: bookId,
            CurrentStock: scenario.stock,
            Location: scenario.location,
            Timestamp: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            
        await daprClient.PublishEventAsync("pubsub", "stock-updates", update);
        
        _logger.LogInformation(
            "Published stock alert: Book {BookId} has {Stock} units in {Location}",
            bookId, scenario.stock, scenario.location);
            
        await Task.Delay(1000); 
    }

    return Ok("Stock updates published");
}
}
