using Microsoft.EntityFrameworkCore;
using aggregateBy.Models;

namespace aggregateBy.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext>options) : base(options) {}

    public DbSet<Product> Products { get; set; }
}
