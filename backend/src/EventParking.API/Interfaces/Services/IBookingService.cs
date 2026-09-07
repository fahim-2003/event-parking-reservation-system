using EventParking.API.DTOs.Bookings;

namespace EventParking.API.Interfaces.Services;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(
        CreateBookingRequest request,
        string customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookingResponse>> GetForCustomerAsync(
        string customerId,
        CancellationToken cancellationToken = default);

    Task<BookingResponse?> GetByIdAsync(
        int bookingId,
        string customerId,
        CancellationToken cancellationToken = default);

    Task<BookingResponse> CancelAsync(
        int bookingId,
        string customerId,
        CancellationToken cancellationToken = default);

    Task<int> ExpireHeldBookingsAsync(
        CancellationToken cancellationToken = default);
}
