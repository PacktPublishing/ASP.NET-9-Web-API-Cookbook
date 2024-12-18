using CountBy.Models;
namespace CountBy.Services;

public interface IProductReadService {
    Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
    Task<IReadOnlyCollection<CategoryDTO>> GetCategoryInfoAsync();

}
