namespace EventParking.API.DTOs.Bookings;

public class BookingResponse
{
    public int Id { get; set; }

    public string BookingNumber { get; set; } = string.Empty;

    public int EventId { get; set; }

    public int? SeatId { get; set; }

    public int? ParkingSlotId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;
}
