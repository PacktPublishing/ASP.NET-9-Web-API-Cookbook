using cookbook.Models;
namespace cookbook.Services;

public interface IProductsService {
    Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
    Task<ProductDTO?> GetAProductAsync(int id);
}