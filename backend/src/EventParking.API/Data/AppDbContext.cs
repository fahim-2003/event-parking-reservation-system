using EventParking.API.Configurations;
using EventParking.API.Entities;
using EventParking.API.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Data;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Venue> Venues => Set<Venue>();

    public DbSet<EventCategory> EventCategories => Set<EventCategory>();

    public DbSet<Event> Events => Set<Event>();

    public DbSet<Seat> Seats => Set<Seat>();

    public DbSet<ParkingSlot> ParkingSlots => Set<ParkingSlot>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();

    public DbSet<ParkingReservation> ParkingReservations => Set<ParkingReservation>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(
            new ApplicationUserConfiguration());

        builder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}
