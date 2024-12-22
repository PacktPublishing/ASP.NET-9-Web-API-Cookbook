using AggregateBy.Models;
namespace AggregateBy.Services;

public interface IProductReadService {
    Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
    Task<PagedProductResponseDTO> GetPagedProductsAsync(int pageSize, int? lastProductId = null);

}
