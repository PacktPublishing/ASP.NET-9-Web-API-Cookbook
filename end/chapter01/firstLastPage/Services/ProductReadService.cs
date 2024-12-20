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

        // First page request
        if (lastProductId == null)
        {
            var firstPageProducts = new List<Product>();
            for (var i = 1; i <= pageSize; i++)
            {
                var product = await context.Products.FindAsync(i);
                if (product != null)
                {
                    firstPageProducts.Add(product);
                }
            }

            return new PagedProductResponseDTO
            {
                Items = firstPageProducts.Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryId = p.CategoryId
                }).ToList(),
                PageSize = pageSize,
                HasPreviousPage = false,
                HasNextPage = firstPageProducts.Count == pageSize,
                TotalPages = totalPages
            };
        }

        // Last page request
        if (lastProductId == ((totalPages - 1) * pageSize))
        {
            var lastPageProducts = new List<Product>();
            for (var i = lastProductId.Value; i < lastProductId.Value + pageSize; i++)
            {
                var product = await context.Products.FindAsync(i);
                if (product != null)
                {
                    lastPageProducts.Add(product);
                }
            }

            return new PagedProductResponseDTO
            {
                Items = lastPageProducts.Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryId = p.CategoryId
                }).ToList(),
                PageSize = pageSize,
                HasPreviousPage = true,
                HasNextPage = false,
                TotalPages = totalPages
            };
        }

        // Regular keyset pagination with tracking
        IQueryable<Product> query = context.Products;
        if (lastProductId.HasValue)
        {
            query = query.Where(p => p.Id > lastProductId.Value);
        }

        var pagedProducts = await query
            .OrderBy(p => p.Id)
            .Take(pageSize)
            .ToListAsync();

        var lastId = pagedProducts.LastOrDefault()?.Id;
        var hasNextPage = lastId.HasValue && 
            await context.Products.AnyAsync(p => p.Id > lastId);

        return new PagedProductResponseDTO
        {
            Items = pagedProducts.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId
            }).ToList(),
            PageSize = pageSize,
            HasPreviousPage = lastProductId.HasValue,
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
