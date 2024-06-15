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

     public async Task<PagedProductResponseDTO> GetPagedProductsAsync(int pageSize, int? lastProductId = null)
    {
        var query = _context.Products.AsQueryable();

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
        var hasNextPage = await _context.Products.AnyAsync(p => p.Id > lastId);

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