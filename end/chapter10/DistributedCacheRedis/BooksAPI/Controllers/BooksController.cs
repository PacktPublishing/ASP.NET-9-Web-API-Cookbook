using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Mvc;
using Books.Services;
using Books.Models;
using System.Text.Json;
using StackExchange.Redis;

namespace Books.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksService _service;
    private readonly IDistributedCache _cache;
    private readonly ILogger<BooksController> _logger;

    public BooksController(
        IBooksService booksService,
        IDistributedCache cache,
        ILogger<BooksController> logger)
    {
        _service = booksService ?? throw new ArgumentNullException(nameof(booksService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            // Generate a unique cache key based on the pagination parameters
            string cacheKey = $"GetBooks_{pageSize}_{lastId}";

            PagedResult<BookDTO>? pagedResult = null;

            // Attempt to retrieve cached data
            string? cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("Fetching books from cache.");

                pagedResult = JsonSerializer.Deserialize<PagedResult<BookDTO>>(cachedData);

                if (pagedResult == null)
                {
                    _logger.LogWarning("Cached data was invalid. Fetching from database.");
                }
            }

            if (pagedResult == null)
            {
                _logger.LogInformation("Fetching books from database.");

                // Simulate delay
                await Task.Delay(5000);

                // Fetch data from the service
                pagedResult = await _service.GetBooksAsync(pageSize, lastId, Url);

                // Serialize and cache the data
                var serializedData = JsonSerializer.Serialize(pagedResult);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // Adjust as needed
                };

                await _cache.SetStringAsync(cacheKey, serializedData, cacheOptions);
            }

            // Add the pagination metadata to the response headers
            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var paginationMetadata = new
            {
                pagedResult.PageSize,
                pagedResult.HasPreviousPage,
                pagedResult.HasNextPage,
                pagedResult.PreviousPageUrl,
                pagedResult.NextPageUrl,
            };

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata, options));

            return Ok(pagedResult.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting books.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
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
            await InvalidateGetBooksCacheAsync();

            var createdBook = await _service.CreateBookAsync(bookDto);

            Response.Headers.Append("X-Books-Modified", "true");

            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while creating a new book.");
        }
    }

    private async Task InvalidateGetBooksCacheAsync()
    {
        var connectionMultiplexer = HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var server = connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().First());

        foreach (var key in server.Keys(pattern: "GetBooks_*"))
        {
            await _cache.RemoveAsync(key);
        }
    }
}
