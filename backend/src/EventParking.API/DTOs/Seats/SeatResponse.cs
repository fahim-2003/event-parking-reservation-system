namespace EventParking.API.DTOs.Seats;

public sealed class SeatResponse
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string RowLabel { get; set; } = string.Empty;

    public int SeatNumber { get; set; }

    public string DisplayLabel { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public byte[] RowVersion { get; set; } = [];
}
