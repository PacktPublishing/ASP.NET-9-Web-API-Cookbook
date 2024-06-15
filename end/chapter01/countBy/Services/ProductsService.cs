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

    public async Task<IReadOnlyCollection<CategoryDTO>> GetCategoryInfoAsync()
    {
	var products = await _context.Products.AsNoTracking().ToListAsync();

        var productsByCategory = products.CountBy(p => p.CategoryId).OrderBy(x => x.Key);
        return productsByCategory.Select(categoryGroup => new CategoryDTO
        {
            CategoryId = categoryGroup.Key,
            ProductCount = categoryGroup.Value
        }).ToList();
    }

}
