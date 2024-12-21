using Microsoft.EntityFrameworkCore;
using ProblemDetailsDemo.Data;
using ProblemDetailsDemo.Models;
using ProblemDetailsDemo.Services;

public class ProductReadService(AppDbContext context) : IProductsService
{
    public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
    {
        return await context.Products
            .AsNoTracking()
            .Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId
            })
            .ToListAsync();
    }

    public async Task<ProductDTO?> GetAProductAsync(int id)
    {
        return await context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId
            })
            .FirstOrDefaultAsync();
    }
}
