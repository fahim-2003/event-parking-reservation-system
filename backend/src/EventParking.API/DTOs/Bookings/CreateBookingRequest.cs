namespace EventParking.API.DTOs.Bookings;

public class CreateBookingRequest
{
    public int EventId { get; set; }

    public int? SeatId { get; set; }

    public int? ParkingSlotId { get; set; }
}
