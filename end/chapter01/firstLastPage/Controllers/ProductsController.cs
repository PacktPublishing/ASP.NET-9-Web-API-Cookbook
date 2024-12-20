using Microsoft.AspNetCore.Mvc;
using FirstLastPage.Models;
using FirstLastPage.Services;
using System.Text.Json;

namespace FirstLastPage.Controllers;

[Route("[controller]")]
[ApiController]
public class ProductsController(
    IProductReadService productReadService,
    ILogger<ProductsController> logger) : ControllerBase
{
    // GET: /Products/AllAtOnce
    [HttpGet("AllAtOnce")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDTO>))] 
    [ProducesResponseType(StatusCodes.Status204NoContent)] 
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAllProducts()
    {
        logger.LogInformation("Retrieving all products");

        try 
        {
            var products = await productReadService.GetAllProductsAsync();

            if (!products.Any())
                return NoContent();

            return Ok(products);
        } 
        catch (Exception ex) 
        {
            logger.LogError(ex, "An error occurred while retrieving all products");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    // GET: /Products
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDTO>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]  // Added for pageSize validation
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts(int pageSize, int? lastProductId = null)
    {
        if (pageSize <= 0)
        {
            return BadRequest("pageSize must be greater than 0");
        }

        var pagedResult = await productReadService.GetPagedProductsAsync(pageSize, lastProductId);

        var previousPageUrl = pagedResult.HasPreviousPage
                ? Url.Action("GetProducts", new { pageSize, lastProductId = pagedResult.Items.First().Id })
                : null;

            var nextPageUrl = pagedResult.HasNextPage
                ? Url.Action("GetProducts", new { pageSize, lastProductId = pagedResult.Items.Last().Id })
                : null;

        var paginationMetadata = new
        {
            PageSize = pagedResult.PageSize,
            HasPreviousPage = pagedResult.HasPreviousPage,
            HasNextPage = pagedResult.HasNextPage,
            PreviousPageUrl = previousPageUrl,
            NextPageUrl = nextPageUrl,
            FirstPageUrl = Url.Action("GetProducts", new { pageSize }),
            LastPageUrl = Url.Action("GetProducts", new { pageSize, lastProductId = (pagedResult.TotalPages - 1) * pageSize })

        };

        var options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata, options));

        return Ok(pagedResult.Items);
    }
}
