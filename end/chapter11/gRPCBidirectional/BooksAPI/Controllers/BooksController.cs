using Microsoft.AspNetCore.Mvc;
using Books.Services;
using Books.Models;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using InventoryService;
using Grpc.Core;

namespace Books.Controllers;

[Route("api/[controller]")]

[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksService _service;
    private readonly Inventory.InventoryClient _inventoryClient;
    private readonly ILogger<BooksController> _logger;

    public BooksController(
            IBooksService booksService, 
            Inventory.InventoryClient inventoryClient,
            ILogger<BooksController> logger)
    {
            _service = booksService ?? throw new ArgumentNullException(nameof(booksService));
            _logger = logger;
            _inventoryClient = inventoryClient;
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

        [HttpGet("{id}/stock")]
        [EndpointSummary("Monitor stock levels for a book")]
        [EndpointDescription("Streams stock updates for a specific book ID.")]
        [ProducesResponseType(StatusCodes.Status200OK)]    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MonitorStock(int id)
        {
            try
            {
                _logger.LogInformation("Starting stock monitoring for Book ID: {BookId}", id);

                using var call = _inventoryClient.MonitorStock();

                await call.RequestStream.WriteAsync(new StockRequest { BookId = id });
                await call.RequestStream.CompleteAsync();

                var stockUpdates = new List<StockUpdate>();
                await foreach (var update in call.ResponseStream.ReadAllAsync())
                {
                    _logger.LogInformation(
                        "Received stock update for Book ID: {BookId} - Stock: {Stock}, Location: {Location}, Timestamp: {Timestamp}",
                        id, update.CurrentStock, update.Location, update.Timestamp);

                    stockUpdates.Add(update);
                }

                return Ok(stockUpdates);
            }
            catch (RpcException rpcEx)
            {
                _logger.LogError(rpcEx, "gRPC error while monitoring stock for Book ID: {BookId}", id);
                return StatusCode(500, "An error occurred while monitoring stock levels.");
            }

        }

        [HttpGet("monitor")]  
        public async Task<IActionResult> MonitorMultipleStocks(CancellationToken cancellationToken)
        {
            try
            {
                using var call = _inventoryClient.MonitorStock();
                var stockUpdates = new List<StockUpdate>();

                var sendTask = Task.Run(async () =>
                {
                    for (int bookId = 1; bookId <= 3; bookId++)
                    {
                        await call.RequestStream.WriteAsync(new StockRequest { BookId = bookId });
                        _logger.LogInformation("Sent stock request for Book ID: {BookId}", bookId);
                        await Task.Delay(1000);
                    }
                    await call.RequestStream.CompleteAsync();
                });

                var receiveTask = Task.Run(async () =>
                {
                    await foreach (var update in call.ResponseStream.ReadAllAsync(cancellationToken))
                    {
                        stockUpdates.Add(update);
                        _logger.LogInformation(
                            "Received stock update for Book ID: {BookId}, Stock: {Stock}", 
                            update.BookId, 
                            update.CurrentStock);
                    }
                });

                await Task.WhenAll(sendTask, receiveTask);
                return Ok(stockUpdates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring multiple stocks");
                return StatusCode(500, "An error occurred while monitoring stocks");
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

            var response = await _inventoryClient.InitializeInventoryAsync(
            new InitializeInventoryRequest
            {
                BookId = createdBook.Id,
                Isbn = createdBook.ISBN,
                InitialStock = 10
            });
            
             _logger.LogInformation(
                "Inventory initialized successfully. InventoryId: {InventoryId}", response.InventoryId);

            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to initialize inventory for Book");
            return StatusCode(500, "An error occurred while creating a new book.");
        }
    }
}
