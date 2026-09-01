using System.ComponentModel.DataAnnotations;

namespace EventParking.API.DTOs.Venues;

public sealed class UpdateVenueRequest
{
    [Required]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name must not be empty or whitespace.")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@".*\S.*", ErrorMessage = "Address must not be empty or whitespace.")]
    public string Address { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0.")]
    public int Capacity { get; set; }
}