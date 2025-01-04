using Microsoft.EntityFrameworkCore;
using FluentValidation.Models;

namespace FluentValidation.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext>options) : base(options) {}

    public DbSet<EventRegistration>? EventRegistrations { get; set; }
}
