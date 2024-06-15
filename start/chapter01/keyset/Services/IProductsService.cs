using cookbook.Models;
namespace cookbook.Services;

public interface IProductsService {
    Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
}