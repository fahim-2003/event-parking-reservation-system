namespace EventParking.API.DTOs.Payments;

public sealed class PaymentResponse
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
