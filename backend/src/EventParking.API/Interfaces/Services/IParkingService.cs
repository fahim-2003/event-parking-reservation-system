using EventParking.API.DTOs.Parking;

namespace EventParking.API.Interfaces.Services;

public interface IParkingService
{
    Task<IReadOnlyList<ParkingSlotResponse>> GetByEventAsync(
        int eventId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ParkingSlotResponse>> GenerateAsync(
        int eventId,
        GenerateParkingLayoutRequest request,
        CancellationToken cancellationToken = default);

    Task<ParkingSlotResponse?> UpdateAsync(
        int eventId,
        int parkingSlotId,
        UpdateParkingSlotRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int eventId,
        int parkingSlotId,
        byte[] rowVersion,
        CancellationToken cancellationToken = default);
}
