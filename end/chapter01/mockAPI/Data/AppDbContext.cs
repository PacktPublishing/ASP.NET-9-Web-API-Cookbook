using Microsoft.EntityFrameworkCore;
using mockAPI.Models;

namespace mockAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Product> Products { get; set; }
} 
