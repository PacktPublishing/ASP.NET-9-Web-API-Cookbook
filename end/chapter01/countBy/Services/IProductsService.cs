using cookbook.Models;
namespace cookbook.Services;

public interface IProductsService {
    Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
    Task<IReadOnlyCollection<CategoryDTO>> GetCategoryInfoAsync();

}