using System.ComponentModel.DataAnnotations;

namespace EventParking.API.DTOs.Parking;

public sealed class UpdateParkingSlotRequest
{
    [Required]
    [RegularExpression(
        @".*\S.*",
        ErrorMessage = "Zone must not be empty or whitespace.")]
    public string Zone { get; set; } = string.Empty;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "SlotNumber must be greater than 0.")]
    public int SlotNumber { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "RowVersion is required.")]
    public byte[] RowVersion { get; set; } = [];
}
