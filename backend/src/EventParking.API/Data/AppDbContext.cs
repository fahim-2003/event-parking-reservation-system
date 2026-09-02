using EventParking.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Venue> Venues => Set<Venue>();

    public DbSet<EventCategory> EventCategories => Set<EventCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
