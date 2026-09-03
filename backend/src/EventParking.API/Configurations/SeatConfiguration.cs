using EventParking.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParking.API.Configurations;

public sealed class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("Seats", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Seats_SeatNumber_Positive",
                "[SeatNumber] > 0");

            tableBuilder.HasCheckConstraint(
                "CK_Seats_Status_Valid",
                "[Status] IN ('Available', 'Held', 'Booked')");
        });

        builder.HasKey(seat => seat.Id);

        builder.Property(seat => seat.RowLabel)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(seat => seat.SeatNumber)
            .IsRequired();

        builder.Property(seat => seat.DisplayLabel)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(seat => seat.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(seat => seat.RowVersion)
            .IsRowVersion();

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(seat => seat.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(seat => new
            {
                seat.EventId,
                seat.DisplayLabel
            })
            .IsUnique();

        builder.HasIndex(seat => new
            {
                seat.EventId,
                seat.Status
            });
    }
}
