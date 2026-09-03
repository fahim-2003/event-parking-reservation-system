namespace EventParking.API.Entities;

public class Booking
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int? SeatId { get; set; }

    public int? ParkingSlotId { get; set; }

    public string CustomerId { get; set; } = string.Empty;

    public string Status { get; set; } = "Confirmed";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public byte[] RowVersion { get; set; } = [];
}
