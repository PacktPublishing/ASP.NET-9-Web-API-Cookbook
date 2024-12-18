using CORS.Data;
using CORS.Models;
using CORS.Services;
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

     public async Task<PagedProductResponseDTO> GetPagedProductsAsync(int pageSize, int? lastProductId = null)
    {
        var query = context.Products.AsNoTracking().AsQueryable();

        if (lastProductId.HasValue)
        {
            query = query.Where(p => p.Id > lastProductId.Value);
        }

        var pagedProducts = await query
            .OrderBy(p => p.Id)
            .Take(pageSize)
            .Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId
            })
            .ToListAsync();

        var lastId = pagedProducts.LastOrDefault()?.Id;
        var hasNextPage = await context.Products.AnyAsync(p => p.Id > lastId);

        var result = new PagedProductResponseDTO
        {
            Items = pagedProducts,
            PageSize = pageSize,
            HasNextPage = hasNextPage,
            HasPreviousPage = lastProductId.HasValue
        };

        return result;
    }
}
