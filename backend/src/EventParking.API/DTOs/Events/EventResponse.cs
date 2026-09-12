namespace EventParking.API.DTOs.Events;

public sealed class EventResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int VenueId { get; set; }

    public string VenueName { get; set; } = string.Empty;

    public string VenueAddress { get; set; } = string.Empty;

    public int EventCategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public DateTime StartDateTimeUtc { get; set; }

    public DateTime EndDateTimeUtc { get; set; }

    public decimal TicketPrice { get; set; }

    public decimal ParkingFee { get; set; }

    public int Capacity { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
