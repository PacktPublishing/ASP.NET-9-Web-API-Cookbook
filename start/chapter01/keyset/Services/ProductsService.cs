using cookbook.Data;
using cookbook.Models;
using cookbook.Services;
using Microsoft.EntityFrameworkCore;

public class ProductReadService : IProductsService
{
    private readonly AppDbContext _context;

    public ProductReadService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
    {
        return await _context.Products
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
}