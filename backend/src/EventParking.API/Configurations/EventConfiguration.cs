using EventParking.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParking.API.Configurations;

public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Events_EndAfterStart",
                "[EndDateTimeUtc] > [StartDateTimeUtc]");

            tableBuilder.HasCheckConstraint(
                "CK_Events_Capacity_Positive",
                "[Capacity] > 0");

            tableBuilder.HasCheckConstraint(
                "CK_Events_TicketPrice_NonNegative",
                "[TicketPrice] >= 0");

            tableBuilder.HasCheckConstraint(
                "CK_Events_ParkingFee_NonNegative",
                "[ParkingFee] >= 0");
        });

        builder.HasKey(eventEntity => eventEntity.Id);

        builder.Property(eventEntity => eventEntity.Name)
            .IsRequired();

        builder.Property(eventEntity => eventEntity.Description)
            .IsRequired(false);

        builder.Property(eventEntity => eventEntity.ImageUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(eventEntity => eventEntity.StartDateTimeUtc)
            .IsRequired();

        builder.Property(eventEntity => eventEntity.EndDateTimeUtc)
            .IsRequired();

        builder.Property(eventEntity => eventEntity.TicketPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(eventEntity => eventEntity.ParkingFee)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(eventEntity => eventEntity.Capacity)
            .IsRequired();

        builder.Property(eventEntity => eventEntity.CreatedAtUtc)
            .IsRequired();

        builder.Property(eventEntity => eventEntity.UpdatedAtUtc)
            .IsRequired();

        builder.HasOne<Venue>()
            .WithMany()
            .HasForeignKey(eventEntity => eventEntity.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EventCategory>()
            .WithMany()
            .HasForeignKey(eventEntity => eventEntity.EventCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(eventEntity => eventEntity.VenueId);

        builder.HasIndex(eventEntity => eventEntity.EventCategoryId);

        builder.HasIndex(eventEntity => eventEntity.StartDateTimeUtc);
    }
}
