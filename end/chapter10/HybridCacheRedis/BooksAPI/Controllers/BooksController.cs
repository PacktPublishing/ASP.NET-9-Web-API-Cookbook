using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using Books.Services;
using Books.Models;
using System.Text.Json;

namespace Books.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksService _service;
    private readonly HybridCache _cache;
    private readonly ICacheKeyTracker _keyTracker;
    private readonly ILogger<BooksController> _logger;

    public BooksController(
            IBooksService booksService,
            HybridCache cache,
            ICacheKeyTracker keyTracker,
            ILogger<BooksController> logger)
        {
            _service = booksService ?? throw new ArgumentNullException(nameof(booksService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _keyTracker = keyTracker ?? throw new ArgumentNullException(nameof(keyTracker));
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
            string cacheKey = $"GetBooks_{pageSize}_{lastId}";
            _keyTracker.TrackKey(cacheKey);

            var cacheEntryOptions = new HybridCacheEntryOptions 
            {
                Expiration = TimeSpan.FromMinutes(3),
                LocalCacheExpiration = TimeSpan.FromMinutes(1)
            };

            var pagedResult = await _cache.GetOrCreateAsync(
                cacheKey,
                async token =>
                {
                    _logger.LogInformation("Fetching books from database.");
                    await Task.Delay(5000);
                    return await _service.GetBooksAsync(pageSize, lastId, Url);
                },
                cacheEntryOptions,
                cancellationToken: HttpContext.RequestAborted);

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
            var createdBook = await _service.CreateBookAsync(bookDto);
            var keys = _keyTracker.GetKeys();
            if (keys.Any())
            {
                await _cache.RemoveAsync(keys);
            }

            Response.Headers.Append("X-Books-Modified", "true");

            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while creating a new book.");
        }
    }
}
