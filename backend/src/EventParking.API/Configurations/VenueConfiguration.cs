using EventParking.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParking.API.Configurations;

public sealed class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("Venues", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Venues_Capacity_Positive",
                "[Capacity] > 0");
        });

        builder.HasKey(venue => venue.Id);

        builder.Property(venue => venue.Name)
            .IsRequired();

        builder.Property(venue => venue.Address)
            .IsRequired();

        builder.Property(venue => venue.Capacity)
            .IsRequired();

        builder.Property(venue => venue.CreatedAtUtc)
            .IsRequired();

        builder.Property(venue => venue.UpdatedAtUtc)
            .IsRequired();
    }
}