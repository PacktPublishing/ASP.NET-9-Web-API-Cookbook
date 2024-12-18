using CountBy.Data;
using CountBy.Models;
using CountBy.Services;
using Microsoft.EntityFrameworkCore;

public class ProductReadService(AppDbContext context) : IProductReadService
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

    public async Task<IReadOnlyCollection<CategoryDTO>> GetCategoryInfoAsync()
    {
	var products = await context.Products.AsNoTracking().ToListAsync();

        var productsByCategory = products.CountBy(p => p.CategoryId).OrderBy(x => x.Key);
        return productsByCategory.Select(categoryGroup => new CategoryDTO
        {
            CategoryId = categoryGroup.Key,
            ProductCount = categoryGroup.Value
        }).ToList();
    }

}
