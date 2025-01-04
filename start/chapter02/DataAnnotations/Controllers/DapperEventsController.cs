using Microsoft.AspNetCore.Mvc;
using DataAnnotations.Services;
using DataAnnotations.Models;
using System.Text.Json;

namespace DataAnnotations.Controllers;

[Route("api/dapper/[controller]")]
[ApiController]
public class DapperEventsController(IDapperService service, ILogger<DapperEventsController> logger) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Paged Event Registrations")]
    [EndpointDescription("This returns all the event registrations from our SQLite database, using Dapper")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<EventRegistrationDTO>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "pageSize", "lastId" })] 
    public async Task<IActionResult> GetEventRegistrations([FromQuery] int pageSize = 10, [FromQuery] int lastId = 0)
    {
        try
        {
            logger.LogInformation("Fetching event registrations with pageSize: {PageSize}, lastId: {LastId}", pageSize, lastId);
            var pagedResult = await service.GetEventRegistrationsAsync(pageSize, lastId, Url);

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
            var eventRegistration = await service.GetEventRegistrationByIdAsync(id);
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
}
