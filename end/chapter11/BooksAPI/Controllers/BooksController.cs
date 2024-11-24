using Microsoft.AspNetCore.Mvc;
using Books.Services;
using Books.Models;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using InventoryService;

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
