namespace EventParking.API.DTOs.Bookings;

public sealed class BookingResponse
{
    public int Id { get; set; }

    public string BookingNumber { get; set; } = string.Empty;

    public int EventId { get; set; }

    public IReadOnlyList<int> SeatIds { get; set; } = [];

    public int? ParkingSlotId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime HoldExpiresAt { get; set; }
}
