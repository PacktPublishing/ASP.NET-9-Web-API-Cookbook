using Microsoft.AspNetCore.Mvc;
using CountBy.Models;
using CountBy.Services;

namespace CountBy.Controllers;

[Route("[controller]")]
[ApiController]
public class ProductsController(IProductReadService productsService, ILogger<ProductsController> logger)
: ControllerBase
{
    // GET: /Products
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDTO>))] 
    [ProducesResponseType(StatusCodes.Status204NoContent)] 
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAllProducts()
    {
        logger.LogInformation("Retrieving all products");

        try 
        {
            var products = await productsService.GetAllProductsAsync();

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

    // GET: /Products/CategoryInfo
	[HttpGet("CategoryInfo")]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CategoryDTO>))] 
	[ProducesResponseType(StatusCodes.Status204NoContent)] 
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	[ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, NoStore = false)] 
	public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategoryInfo()
	{
		logger.LogInformation("Retrieving Category Info");

		try 
		{
			var products = await productsService.GetCategoryInfoAsync();

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
}
