using EventParking.API.DTOs.Payments;

namespace EventParking.API.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentResponse> CreateAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default);
}
