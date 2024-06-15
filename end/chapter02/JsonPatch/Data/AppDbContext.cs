using Microsoft.EntityFrameworkCore;
using events.Models;

namespace events.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<EventRegistration>? EventRegistrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventRegistration>(entity =>
            {
                entity.OwnsOne(e => e.AdditionalContact, ac =>
                {
                    ac.Property(a => a.PhoneNumber).HasColumnName("PhoneNumber");
                    ac.Property(a => a.Address).HasColumnName("Address");
                });
            });
        }
    }
}

