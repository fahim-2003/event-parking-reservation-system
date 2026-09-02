using EventParking.API.DTOs.Seats;

namespace EventParking.API.Interfaces.Services;

public interface ISeatService
{
    Task<IReadOnlyList<SeatResponse>> GetByEventAsync(
        int eventId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SeatResponse>> GenerateAsync(
        int eventId,
        GenerateSeatMapRequest request,
        CancellationToken cancellationToken = default);

    Task<SeatResponse?> UpdateAsync(
        int eventId,
        int seatId,
        UpdateSeatRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int eventId,
        int seatId,
        byte[] rowVersion,
        CancellationToken cancellationToken = default);
}
