using Microsoft.AspNetCore.Mvc;
using ProblemDetailsDemo.Models;
using ProblemDetailsDemo.Services;

namespace ProblemDetailsDemo.Controllers;

[Route("[controller]")]
[ApiController]
public class ProductsController(IProductsService productsService, ILogger<ProductsController> logger) : ControllerBase
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

    // GET: /Products/1
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDTO>> GetAProduct(int id)
    {
        logger.LogInformation($"Retrieving product with id {id}");

        try 
        {
            var product = await productsService.GetAProductAsync(id);

            if (product == null)
                return NotFound();

            return Ok(product);
        } 
        catch (Exception ex) 
        {
            logger.LogError(ex, $"An error occurred while retrieving product with id {id}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
