using System.ComponentModel.DataAnnotations;

namespace EventParking.API.DTOs.Seats;

public sealed class UpdateSeatRequest
{
    [Required]
    [RegularExpression(
        @".*\S.*",
        ErrorMessage = "RowLabel must not be empty or whitespace.")]
    public string RowLabel { get; set; } = string.Empty;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "SeatNumber must be greater than 0.")]
    public int SeatNumber { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "RowVersion is required.")]
    public byte[] RowVersion { get; set; } = [];
}
