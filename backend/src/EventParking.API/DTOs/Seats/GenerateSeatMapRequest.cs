using System.ComponentModel.DataAnnotations;

namespace EventParking.API.DTOs.Seats;

public sealed class GenerateSeatMapRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Rows must be greater than 0.")]
    public int Rows { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "SeatsPerRow must be greater than 0.")]
    public int SeatsPerRow { get; set; }
}
