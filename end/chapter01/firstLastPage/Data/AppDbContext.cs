using Microsoft.EntityFrameworkCore;
using FirstLastPage.Models;

namespace FirstLastPage.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext>options) : base(options) {}

    public DbSet<Product> Products { get; set; }
}
