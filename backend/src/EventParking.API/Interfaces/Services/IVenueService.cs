using EventParking.API.DTOs.Venues;

namespace EventParking.API.Interfaces.Services;

public interface IVenueService
{
    Task<IReadOnlyList<VenueResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<VenueResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<VenueResponse> CreateAsync(
        CreateVenueRequest request,
        CancellationToken cancellationToken = default);

    Task<VenueResponse?> UpdateAsync(
        int id,
        UpdateVenueRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}