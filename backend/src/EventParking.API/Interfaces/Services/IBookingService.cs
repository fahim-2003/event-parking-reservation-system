using EventParking.API.DTOs.Bookings;

namespace EventParking.API.Interfaces.Services;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(
        CreateBookingRequest request,
        string customerId,
        CancellationToken cancellationToken = default);
}
