using Dapr;
using Microsoft.AspNetCore.Mvc;
using Books.Services;
using Books.Models;
using System.Text.Json;
using Dapr.Client;
using StackExchange.Redis;

namespace Books.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksService _service;
    private readonly ILogger<BooksController> _logger;
    private readonly DaprClient _daprClient;

    public BooksController(
        IBooksService booksService,
        DaprClient daprClient, 
        ILogger<BooksController> logger)
    {
        _service = booksService ?? throw new ArgumentNullException(nameof(booksService));
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        _logger = logger;
    }

    [HttpGet]
    [EndpointSummary("Paged Book Information")]
    [EndpointDescription("This returns all the books from our SQLite database, using EF Core")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<BookDTO>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBooks([FromQuery] int pageSize = 10, [FromQuery] int lastId = 0)
    {
        try
        {
            await Task.Delay(5000);

            var pagedResult = await _service.GetBooksAsync(pageSize, lastId, Url);

            var paginationMetadata = new
            {
                pagedResult.PageSize,
                pagedResult.HasPreviousPage,
                pagedResult.HasNextPage,
                pagedResult.PreviousPageUrl,
                pagedResult.NextPageUrl,
            };

            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata, options));

            return Ok(pagedResult.Items);
        }
        catch (Exception) 
        {
            return StatusCode(500, "An error occurred while fetching event registrations.");
        }
    }

    [HttpGet("{id}")]
    [EndpointSummary("Get a book by Id")]
    [EndpointDescription("Returns a single book by its Id from our SQLite database, using EF Core")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBookById(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Id must be greater than 0");
        }

        try
        {
            var book = await _service.GetBookByIdAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching event registration by Id.");
        }
    }

    [HttpPost]
    [EndpointSummary("Create a new book")]
    [EndpointDescription("POST to create a new book.  Accepts a BookDTO.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBook([FromBody] BookDTO bookDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            _logger.LogInformation("Creating new book with ISBN: {ISBN}", bookDto.ISBN);

            var createdBook = await _service.CreateBookAsync(bookDto);

            _logger.LogInformation(
                "Book created successfully with ID: {BookId}", createdBook.Id);

            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to initialize inventory for Book");
            return StatusCode(500, "An error occurred while creating a new book.");
        }
    }

    [Topic("pubsub", "stock-updates")]
    [HttpPost("stock-updates")]
    public async Task<IActionResult> HandleStockUpdateAsync([FromBody] StockUpdate update)
    {
        var stateKey = $"book-{update.BookId}-stock";

        await _daprClient.SaveStateAsync(
        "statestore",
        stateKey,
        update);

        _logger.LogInformation(
         "Stored stock update for Book {BookId}: {Stock} units",
         update.BookId,
         update.CurrentStock);


        var alertLevel = update.CurrentStock switch
        {
            <= 10 => "CRITICAL",
            <= 25 => "LOW",
            <= 50 => "MODERATE",
            _ => "HEALTHY"
        };

        _logger.LogInformation(
            "Stock Alert [{Level}]: Book {BookId} has {Stock} units in {Location}",
            alertLevel,
            update.BookId,
            update.CurrentStock,
            update.Location);

            return Ok(new
            {
                bookId = update.BookId,
                stock = update.CurrentStock,
                location = update.Location,
                alertLevel,
                timestamp = DateTimeOffset.FromUnixTimeSeconds(update.Timestamp)
            });
        }

    [HttpGet("stock-log/{bookId}")]
    public async Task<IActionResult> GetStockHistory(int bookId)
    {
        var stateKey = $"book-{bookId}-stock";
        
        try
        {
            _logger.LogInformation("Attempting to get state for key: {Key}", stateKey);
            
            var stockUpdate = await _daprClient.GetStateAsync<Object>(
                "statestore", 
                stateKey);
                
            // Add verbose logging
            _logger.LogInformation("Retrieved value: {@Value}", stockUpdate);

            if (stockUpdate == null)
                return NotFound($"No stock history for book {bookId}");

            return Ok(stockUpdate);
        }
        catch (Exception ex)
        {
            // Log the full exception details
            _logger.LogError(ex, "Detailed error getting state. Key: {Key}, Message: {Message}, Stack: {Stack}", 
                stateKey, 
                ex.Message, 
                ex.StackTrace);
                
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {Message}, Stack: {Stack}", 
                    ex.InnerException.Message,
                    ex.InnerException.StackTrace);
            }
            
            return StatusCode(500, new { 
                error = ex.ToString(),
                innerError = ex.InnerException?.ToString(),
                key = stateKey
            });
        }
    } 

    [HttpGet("debug-state/{bookId}")]
    public async Task<IActionResult> DebugState(int bookId)
    {
        try
        {
            // Log all possible key variations we might try
            var possibleKeys = new[]
            {
                $"book-{bookId}-stock",
                $"statestore||book-{bookId}-stock",
                $"inventory||book-{bookId}-stock"
            };

            var results = new Dictionary<string, object>();
            
            foreach (var key in possibleKeys)
            {
                _logger.LogInformation("Trying key: {Key}", key);
                try
                {
                    var value = await _daprClient.GetStateAsync<StockUpdate>("statestore", key);
                    results[key] = value;
                }
                catch (Exception ex)
                {
                    results[key] = $"Error: {ex.Message}";
                }
            }

            return Ok(new
            {
                attemptedKeys = results,
                redisKeys = "Use redis-cli KEYS * output here"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

        

    [HttpGet("stock-updates/{bookId}")]
    public async Task<IActionResult> GetStockUpdates(int bookId)
    {
        try
        {
            var query = "{" +
                $"\"filter\": {{\"bookId\": {bookId}}}, " +
                "\"sort\": [{\"timestamp\": \"DESC\"}]" +
                "}";

            var result = await _daprClient.QueryStateAsync<StockUpdate>(
                "statestore",
                query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying stock updates");
            return StatusCode(500);
        }
    }
}
