using Microsoft.AspNetCore.Mvc;
using cookbook.Models;
using cookbook.Services;

namespace cookbook.Controllers;

[Route("[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
	private readonly IProductsService _productsService;
	private readonly ILogger<ProductsController> _logger; 

    public ProductsController(IProductsService productsService, ILogger<ProductsController> logger)
    {
        _productsService = productsService;
        _logger = logger;
    }

    // GET: /Products
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDTO>))] 
    [ProducesResponseType(StatusCodes.Status204NoContent)] 
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAllProducts()
    {
        _logger.LogInformation("Retrieving all products");

        try 
        {
            var products = await _productsService.GetAllProductsAsync();

            if (!products.Any())
                return NoContent();

            return Ok(products);
        } 
        catch (Exception ex) 
        {
            _logger.LogError(ex, "An error occurred while retrieving all products");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    // GET: /Products/CategoryInfo
	[HttpGet("CategoryInfo")]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CategoryDTO>))] 
	[ProducesResponseType(StatusCodes.Status204NoContent)] 
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	[ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, NoStore = false)] 
	public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategoryInfo()
	{
		_logger.LogInformation("Retrieving Category Info");

		try 
		{
			var products = await _productsService.GetCategoryInfoAsync();

			if (!products.Any())
				return NoContent();

			return Ok(products);
		} 
		catch (Exception ex) 
		{
			_logger.LogError(ex, "An error occurred while retrieving all products");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}
	}
}