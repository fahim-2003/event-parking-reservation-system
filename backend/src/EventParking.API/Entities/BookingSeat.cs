namespace EventParking.API.Entities;

public sealed class BookingSeat
{
    public int BookingId { get; set; }

    public int SeatId { get; set; }

    public Booking Booking { get; set; } = null!;

    public Seat Seat { get; set; } = null!;
}
