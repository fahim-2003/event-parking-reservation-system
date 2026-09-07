namespace EventParking.API.Entities;

public class Booking
{
    public int Id { get; set; }

    public string BookingNumber { get; set; } = string.Empty;

    public int EventId { get; set; }

    public string CustomerId { get; set; } = string.Empty;

    public string Status { get; set; } = "Held";

    public string PaymentStatus { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public ICollection<BookingSeat> BookingSeats { get; set; } =
        new List<BookingSeat>();

    public ParkingReservation? ParkingReservation { get; set; }
}
