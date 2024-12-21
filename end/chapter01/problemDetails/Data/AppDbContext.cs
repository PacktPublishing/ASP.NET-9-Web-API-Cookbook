using Microsoft.EntityFrameworkCore;
using ProblemDetailsDemo.Models;

namespace ProblemDetailsDemo.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext>options) : base(options) {}

    public DbSet<Product> Products { get; set; }
}
