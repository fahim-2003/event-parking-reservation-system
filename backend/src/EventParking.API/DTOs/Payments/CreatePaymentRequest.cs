namespace EventParking.API.DTOs.Payments;

public sealed class CreatePaymentRequest
{
    public int BookingId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;
}
