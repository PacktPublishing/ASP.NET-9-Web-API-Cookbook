using Microsoft.AspNetCore.Mvc;
using Books.Services;
using Books.Models;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;

namespace Books.Controllers;

[Route("api/[controller]")]

[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksService _service;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BooksController> _logger;

    public BooksController(
            IBooksService booksService, 
            IMemoryCache cache,

            ILogger<BooksController> logger)
    {
            _service = booksService ?? throw new ArgumentNullException(nameof(booksService));
            _logger = logger;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    [HttpGet]
    [EndpointSummary("Paged Book Information")]
    [EndpointDescription("This returns all the books from our SQLite database, using EF Core")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<BookDTO>))]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 3, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> GetBooks([FromQuery] int pageSize = 10, [FromQuery] int lastId = 0)
    {
        try
        {
           string? etag;
           if (!_cache.TryGetValue("BooksETag", out etag))
           {
               var cacheOptions = new MemoryCacheEntryOptions()
                   .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                   .SetAbsoluteExpiration(TimeSpan.FromHours(24));
                    
               etag = $"\"{Guid.NewGuid():n}\"";
               _cache.Set("BooksETag", etag, cacheOptions);
            }

            if (Request.Headers.IfNoneMatch.Count > 0)
            {
                var requestETag = Request.Headers.IfNoneMatch.First();
                if (requestETag == etag)
                {
                    return StatusCode(StatusCodes.Status304NotModified);
                }
            };

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
            Response.GetTypedHeaders().ETag = new EntityTagHeaderValue(etag);

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
            
            _cache.Set("BooksETag", $"\"{Guid.NewGuid():n}\"");
            
            Response.Headers.CacheControl = "no-store";

            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while creating a new book.");
        }
    }
}
