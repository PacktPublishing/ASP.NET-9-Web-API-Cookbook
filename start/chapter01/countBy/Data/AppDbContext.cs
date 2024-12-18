using Microsoft.EntityFrameworkCore;
using CountBy.Models;

namespace CountBy.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext>options) : base(options) {}

    public DbSet<Product> Products { get; set; }
}
