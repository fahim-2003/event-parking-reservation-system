namespace EventParking.API.DTOs.Bookings;

public sealed class CreateBookingRequest
{
    public int EventId { get; set; }

    public List<int> SeatIds { get; set; } = [];

    public int? ParkingSlotId { get; set; }
}
