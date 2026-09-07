namespace EventParking.API.Entities;

public sealed class ParkingReservation
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public int ParkingSlotId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Booking Booking { get; set; } = null!;

    public ParkingSlot ParkingSlot { get; set; } = null!;
}
