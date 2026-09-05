using EventParking.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParking.API.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Bookings_Status_Valid",
                "[Status] IN ('Held', 'Confirmed', 'Cancelled', 'Expired')");
        });

        builder.HasKey(booking => booking.Id);

        builder.Property(booking => booking.CustomerId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(booking => booking.Status)
            .HasMaxLength(20)
            .IsRequired();


        builder.Property(booking => booking.BookingNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(booking => booking.PaymentStatus)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(booking => booking.CreatedAt)
            .IsRequired();

        builder.Property(booking => booking.RowVersion)
            .IsRowVersion();

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(booking => booking.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Seat>()
            .WithMany()
            .HasForeignKey(booking => booking.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ParkingSlot>()
            .WithMany()
            .HasForeignKey(booking => booking.ParkingSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(booking => new
            {
                booking.CustomerId,
                booking.EventId
            });

        builder.HasIndex(booking => booking.SeatId)
            .IsUnique();

        builder.HasIndex(booking => booking.ParkingSlotId)
            .IsUnique();
    }
}


