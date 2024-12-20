using FirstLastPage.Data;
using FirstLastPage.Models;
using FirstLastPage.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public class ProductReadService(AppDbContext context, IMemoryCache cache) : IProductReadService
{
    private const string TotalPagesKey = "TotalPages";

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
        var totalPages = await GetTotalPagesAsync(pageSize);
        List<Product> products;
        bool hasNextPage;
        bool hasPreviousPage;

        // First page request
        if (lastProductId == null)
        {
            products = new List<Product>();
            for (var i = 1; i <= pageSize; i++)
            {
                var product = await context.Products.FindAsync(i);
                if (product != null)
                {
                    products.Add(product);
                }
            }
            hasNextPage = products.Count == pageSize;
            hasPreviousPage = false;
        }
        // Last page request
        else if (lastProductId == ((totalPages - 1) * pageSize))
        {
            products = new List<Product>();
            for (var i = lastProductId.Value; i < lastProductId.Value + pageSize; i++)
            {
                var product = await context.Products.FindAsync(i);
                if (product != null)
                {
                    products.Add(product);
                }
            }
            hasNextPage = false;
            hasPreviousPage = true;
        }
        // Regular keyset pagination with tracking
        else
        {
            IQueryable<Product> query = context.Products;
            query = query.Where(p => p.Id > lastProductId.Value);
            products = await query
                .OrderBy(p => p.Id)
                .Take(pageSize)
                .ToListAsync();

            var lastId = products.LastOrDefault()?.Id;
            hasNextPage = lastId.HasValue && 
                await context.Products.AnyAsync(p => p.Id > lastId);
            hasPreviousPage = true;
        }

        return new PagedProductResponseDTO
        {
            Items = products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId
            }).ToList(),
            PageSize = pageSize,
            HasPreviousPage = hasPreviousPage,
            HasNextPage = hasNextPage,
            TotalPages = totalPages
        };
    }

    public async Task<int> GetTotalPagesAsync(int pageSize)
    {
        if (!cache.TryGetValue(TotalPagesKey, out int totalPages))
        {
            var totalCount = await context.Products.CountAsync();
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            cache.Set(TotalPagesKey, totalPages, TimeSpan.FromMinutes(2));
        }
        return totalPages;
    }

    public void InvalidateCache()
    {
        cache.Remove(TotalPagesKey);
    }
}
