using CORS.Models;
namespace CORS.Services;

public interface IProductReadService {
    Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
    Task<PagedProductResponseDTO> GetPagedProductsAsync(int pageSize, int? lastProductId = null);
}
