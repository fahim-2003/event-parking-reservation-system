using System.ComponentModel.DataAnnotations;

namespace EventParking.API.DTOs.Events;

public sealed class CreateEventRequest
{
    [Required]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name must not be empty or whitespace.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "VenueId must be greater than 0.")]
    public int VenueId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "EventCategoryId must be greater than 0.")]
    public int EventCategoryId { get; set; }

    public DateTime StartDateTimeUtc { get; set; }

    public DateTime EndDateTimeUtc { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335",
        ErrorMessage = "TicketPrice must be 0 or greater.")]
    public decimal TicketPrice { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335",
        ErrorMessage = "ParkingFee must be 0 or greater.")]
    public decimal ParkingFee { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0.")]
    public int Capacity { get; set; }
}
