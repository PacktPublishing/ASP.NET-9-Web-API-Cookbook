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

    // GET: /Products/1
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDTO>> GetAProduct(int id)
    {
        _logger.LogInformation($"Retrieving product with id {id}");

        try 
        {
            var product = await _productsService.GetAProductAsync(id);

            if (product == null)
                return NotFound();

            return Ok(product);
        } 
        catch (Exception ex) 
        {
            _logger.LogError(ex, $"An error occurred while retrieving product with id {id}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}