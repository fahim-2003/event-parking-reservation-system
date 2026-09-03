using EventParking.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParking.API.Configurations;

public sealed class ParkingSlotConfiguration : IEntityTypeConfiguration<ParkingSlot>
{
    public void Configure(EntityTypeBuilder<ParkingSlot> builder)
    {
        builder.ToTable("ParkingSlots", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_ParkingSlots_SlotNumber_Positive",
                "[SlotNumber] > 0");

            tableBuilder.HasCheckConstraint(
                "CK_ParkingSlots_Status_Valid",
                "[Status] IN ('Available', 'Held', 'Occupied')");
        });

        builder.HasKey(parkingSlot => parkingSlot.Id);

        builder.Property(parkingSlot => parkingSlot.Zone)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(parkingSlot => parkingSlot.SlotNumber)
            .IsRequired();

        builder.Property(parkingSlot => parkingSlot.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(parkingSlot => parkingSlot.RowVersion)
            .IsRowVersion();

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(parkingSlot => parkingSlot.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(parkingSlot => new
            {
                parkingSlot.EventId,
                parkingSlot.Zone,
                parkingSlot.SlotNumber
            })
            .IsUnique();

        builder.HasIndex(parkingSlot => new
            {
                parkingSlot.EventId,
                parkingSlot.Status
            });
    }
}
