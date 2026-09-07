using EventParking.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParking.API.Configurations;

public sealed class BookingSeatConfiguration :
    IEntityTypeConfiguration<BookingSeat>
{
    public void Configure(EntityTypeBuilder<BookingSeat> builder)
    {
        builder.ToTable("BookingSeats");

        builder.HasKey(bookingSeat => new
        {
            bookingSeat.BookingId,
            bookingSeat.SeatId
        });

        builder.HasOne(bookingSeat => bookingSeat.Booking)
            .WithMany(booking => booking.BookingSeats)
            .HasForeignKey(bookingSeat => bookingSeat.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bookingSeat => bookingSeat.Seat)
            .WithMany()
            .HasForeignKey(bookingSeat => bookingSeat.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(bookingSeat => bookingSeat.SeatId);
    }
}
