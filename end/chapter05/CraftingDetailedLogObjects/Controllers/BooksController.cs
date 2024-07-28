using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using books.Services;
using books.Models;
using System.Text.Json;

namespace books.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksService _service;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IBooksService service, ILogger<BooksController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [EndpointSummary("Paged Book Inforation")]
    [EndpointDescription("This returns all the books from our SQLite database, using EF Core")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<BookDTO>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "pageSize", "lastId" })]

    public async Task<IActionResult> GetBooks([FromQuery] int pageSize = 10, [FromQuery] int lastId = 0)
    {
        using (LogContext.PushProperty("EndpointName", nameof(GetBooks)))
        {
        try
        {
            var pagedResult = await _service.GetBooksAsync(pageSize, lastId, Url);

            var paginationMetadata = new
            {
                pagedResult.PageSize,
                pagedResult.HasPreviousPage,
                pagedResult.HasNextPage,
                pagedResult.PreviousPageUrl,
                pagedResult.NextPageUrl
            };

            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata, options));

            _logger.LogInformation("Retrieved {BookCount} books. Pagination: {@paginationMetadata}", pagedResult.Items!.Count, paginationMetadata);

            _logger.LogInformation("Returning status code {StatusCode}", StatusCodes.Status200OK);

            var logObject = new { 
                QueryParameters = new { PageSize = pageSize, LastId = lastId },     PaginationMetadata = paginationMetadata, 
                BookCount = pagedResult.Items!.Count, 
                FirstBookId = pagedResult.Items.FirstOrDefault()?.Id, 
                LastBookId = pagedResult.Items.LastOrDefault()?.Id, 
                GenreCounts = pagedResult.Items.GroupBy(b => b.Genre)
                    .Select(g => new { Genre = g.Key, Count = g.Count() 
            })
};
            _logger.LogInformation("Books retrieved successfully. Details: {@BookOperationDetails}", logObject);

            return Ok(pagedResult.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching books. QueryParams: {@QueryParameters}", 
            new { pageSize, lastId });
		_logger.LogInformation("Returning status code {StatusCode}", StatusCodes.Status500InternalServerError);

            return StatusCode(500, "An error occurred while fetching event registrations.");
        }
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
            var books = await _service.GetBookByIdAsync(id);

            if (books == null)
            {
                return NotFound();
            }


            return Ok(books);
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
            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while creating a new book.");
        }
    }
}
