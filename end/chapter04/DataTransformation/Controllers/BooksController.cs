using Microsoft.AspNetCore.Mvc;
using Books.Services;
using Books.Models;
using System.Text.Json;

namespace Books.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController(IBooksService service, ILogger<BooksController> logger) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Paged Book Inforation")]
    [EndpointDescription("This returns all the books from our SQLite database, using EF Core")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<BookDTO>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "pageSize", "lastId" })]

    public async Task<IActionResult> GetBooks([FromQuery] int pageSize = 10, [FromQuery] int lastId = 0)
    {
        try
        {
            logger.LogInformation("Fetching event registrations with pageSize: {PageSize}, lastId: {LastId}", pageSize, lastId);
            var pagedResult = await service.GetBooksAsync(pageSize, lastId, Url);

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

            return Ok(pagedResult.Items);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching event registrations.");
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
            var eventRegistration = await service.GetBookByIdAsync(id);
            if (eventRegistration == null)
            {
                return NotFound();
            }

            return Ok(eventRegistration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching event registration by Id: {Id}", id);
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
            var createdBook = await service.CreateBookAsync(bookDto);
            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating a new book.");
            return StatusCode(500, "An error occurred while creating a new book.");
        }
    }
}
