using EventParking.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParking.API.Configurations;

public sealed class ParkingReservationConfiguration :
    IEntityTypeConfiguration<ParkingReservation>
{
    public void Configure(
        EntityTypeBuilder<ParkingReservation> builder)
    {
        builder.ToTable("ParkingReservations");

        builder.HasKey(reservation => reservation.Id);

        builder.Property(reservation => reservation.CreatedAt)
            .IsRequired();

        builder.HasOne(reservation => reservation.Booking)
            .WithOne(booking => booking.ParkingReservation)
            .HasForeignKey<ParkingReservation>(
                reservation => reservation.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(reservation => reservation.ParkingSlot)
            .WithMany()
            .HasForeignKey(reservation => reservation.ParkingSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(reservation => reservation.BookingId)
            .IsUnique();

        builder.HasIndex(reservation => reservation.ParkingSlotId);
    }
}
