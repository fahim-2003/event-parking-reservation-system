using EventParking.API.DTOs.Payments;

namespace EventParking.API.Interfaces.Services;

public interface IPaymentService
{
    // Legacy/internal creation path retained for compatibility.
    Task<PaymentResponse> CreateAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentResponse> ProcessAsync(
        ProcessPaymentRequest request,
        string customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentResponse>> GetForCustomerAsync(
        string customerId,
        CancellationToken cancellationToken = default);
}
