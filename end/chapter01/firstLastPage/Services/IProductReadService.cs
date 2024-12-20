using FirstLastPage.Models;
namespace FirstLastPage.Services;

public interface IProductReadService {
    Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
    Task<PagedProductResponseDTO> GetPagedProductsAsync(int pageSize, int? lastProductId = null);
    Task<int> GetTotalPagesAsync(int pageSize);
    void InvalidateCache();
}
