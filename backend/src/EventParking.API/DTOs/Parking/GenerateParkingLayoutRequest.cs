using System.ComponentModel.DataAnnotations;

namespace EventParking.API.DTOs.Parking;

public sealed class GenerateParkingLayoutRequest
{
    [Required]
    [RegularExpression(
        @".*\S.*",
        ErrorMessage = "Zone must not be empty or whitespace.")]
    public string Zone { get; set; } = string.Empty;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "NumberOfSlots must be greater than 0.")]
    public int NumberOfSlots { get; set; }
}
