using Microsoft.EntityFrameworkCore;
using CORS.Models;

namespace CORS.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext>options) : base(options) {}

    public DbSet<Product> Products { get; set; }
}
