using Microsoft.AspNetCore.Mvc;
using events.Services;
using events.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace events.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly IEventsService _service;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IEventsService service, ILogger<EventsController> logger)
    {
        _service = service;
        _logger = logger;
    }


    [HttpGet]
    [EndpointSummary("Paged Event Registrations")]
    [EndpointDescription("This returns all the event registrations from our SQLite database, using EF Core")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<EventRegistrationDTO>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "pageSize", "lastId" })]
    public async Task<IActionResult> GetEventRegistrations([FromQuery] int pageSize = 10, [FromQuery] int lastId = 0)
    {
        try
        {
            _logger.LogInformation("Fetching event registrations with pageSize: {PageSize}, lastId: {LastId}", pageSize, lastId);
            var pagedResult = await _service.GetEventRegistrationsAsync(pageSize, lastId, Url);

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
            _logger.LogError(ex, "An error occurred while fetching event registrations.");
            return StatusCode(500, "An error occurred while fetching event registrations.");
        }
    }

    [HttpGet("{id}")]
    [EndpointSummary("Get a event by Id")]
    [EndpointDescription("Returns a single event registration by its Id from our SQLite database, using EF Core")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EventRegistrationDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEventRegistrationById(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Id must be greater than 0");
        }

        try
        {
            var eventRegistration = await _service.GetEventRegistrationByIdAsync(id);
            if (eventRegistration == null)
            {
                return NotFound();
            }

            return Ok(eventRegistration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching event registration by Id: {Id}", id);
            return StatusCode(500, "An error occurred while fetching event registration by Id.");
        }
    }

    [HttpPost]
    [EndpointSummary("Create a new event registration")]
    [EndpointDescription("POST to create a new event registration.  Accepts a EventRegisrationDTO.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostEventRegistration([FromBody] EventRegistrationDTO eventRegistrationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdEvent = await _service.CreateEventRegistrationAsync(eventRegistrationDto);

        return CreatedAtAction(nameof(GetEventRegistrationById), new { id = createdEvent.Id }, createdEvent);

    }

    [HttpGet("authtest")]
    public IActionResult AuthTest()
    {
        return Ok("This endpoint is secured");
    }

}
