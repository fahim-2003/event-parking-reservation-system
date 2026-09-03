namespace EventParking.API.Entities;

public class Seat
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string RowLabel { get; set; } = string.Empty;

    public int SeatNumber { get; set; }

    public string DisplayLabel { get; set; } = string.Empty;

    public string Status { get; set; } = "Available";

    public byte[] RowVersion { get; set; } = [];
}
