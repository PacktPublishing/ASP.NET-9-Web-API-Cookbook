using Microsoft.EntityFrameworkCore;
using Sales.Models;

namespace Sales.Data;

public class SalesDbContext : DbContext 
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Lines)
            .WithOne(l => l.Order)
            .HasForeignKey(l => l.OrderId);

        modelBuilder.Entity<OrderLine>()
            .Property(l => l.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.Total)
            .HasPrecision(18, 2);
    }
}
