using ProblemDetailsDemo.Models;
namespace ProblemDetailsDemo.Services;

public interface IProductsService {
    Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
    Task<ProductDTO?> GetAProductAsync(int id);
}
