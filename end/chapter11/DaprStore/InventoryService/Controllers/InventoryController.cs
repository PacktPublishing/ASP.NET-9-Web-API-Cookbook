using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using InventoryService.Models;
using System.Text.Json;

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

            // Add metadata to ensure JSON storage
            var metadata = new Dictionary<string, string> 
            { 
                { "contentType", "application/json" }
            };
                
            await daprClient.SaveStateAsync(
                "statestore", 
                $"book-{bookId}-stock", 
                update,
                metadata: metadata);
                
            await daprClient.PublishEventAsync("pubsub", "stock-updates", update);
            
            _logger.LogInformation(
                "Published stock alert: Book {BookId} has {Stock} units in {Location}",
                bookId, scenario.stock, scenario.location);
                
            await Task.Delay(1000); 
        }

        return Ok("Stock updates published");
    }

    [HttpPost("test-stock/{bookId}")]
    public async Task<IActionResult> TestStockUpdate(int bookId)
    {
        var random = new Random();
        var stock = random.Next(1, 101);
        var locations = new[] { "Main Warehouse", "Store Front", "Online Fulfillment", "Reserve Stock" };
        var location = locations[random.Next(locations.Length)];
        
        var update = new StockUpdate(
            BookId: bookId,
            CurrentStock: stock,
            Location: location,
            Timestamp: DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        var jsonData = JsonSerializer.Serialize(update);
        var metadata = new Dictionary<string, string> 
        { 
            { "contentType", "application/json" }
        };
            
        await daprClient.SaveStateAsync(
            "statestore", 
            $"book-{bookId}-stock", 
            jsonData,
            metadata: metadata);
        
        _logger.LogInformation(
            "Saved stock update: Book {BookId} has {Stock} units in {Location}",
            bookId, stock, location);

        return Ok(new { 
            message = "Stock update saved",
            update = update 
        });
    }

    
}
