namespace EventParking.API.DTOs.Payments;

public sealed class ProcessPaymentRequest
{
    public int BookingId { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;
}
