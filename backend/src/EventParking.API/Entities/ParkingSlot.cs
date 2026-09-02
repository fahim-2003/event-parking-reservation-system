namespace EventParking.API.Entities;

public class ParkingSlot
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string Zone { get; set; } = string.Empty;

    public int SlotNumber { get; set; }

    public string Status { get; set; } = "Available";

    public byte[] RowVersion { get; set; } = [];
}
