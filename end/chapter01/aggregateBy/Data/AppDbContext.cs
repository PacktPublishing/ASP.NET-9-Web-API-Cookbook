using Microsoft.EntityFrameworkCore;
using AggregateBy.Models;

namespace AggregateBy.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext>options) : base(options) {}

    public DbSet<Product> Products { get; set; }
}
