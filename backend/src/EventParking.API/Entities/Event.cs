namespace EventParking.API.Entities;

public class Event
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int VenueId { get; set; }

    public int EventCategoryId { get; set; }

    public DateTime StartDateTimeUtc { get; set; }

    public DateTime EndDateTimeUtc { get; set; }

    public decimal TicketPrice { get; set; }

    public decimal ParkingFee { get; set; }

    public int Capacity { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
