namespace EventParking.API.DTOs.Parking;

public sealed class ParkingSlotResponse
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string Zone { get; set; } = string.Empty;

    public int SlotNumber { get; set; }

    public string Status { get; set; } = string.Empty;

    public byte[] RowVersion { get; set; } = [];
}
