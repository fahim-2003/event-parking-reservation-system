using EventParking.API.DTOs.Events;

namespace EventParking.API.Interfaces.Services;

public interface IEventService
{
    Task<IReadOnlyList<EventResponse>> GetAllAsync(
        string? search = null,
        DateTime? dateFromUtc = null,
        DateTime? dateToUtc = null,
        int? venueId = null,
        int? categoryId = null,
        CancellationToken cancellationToken = default);

    Task<EventResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<EventResponse> CreateAsync(
        CreateEventRequest request,
        CancellationToken cancellationToken = default);

    Task<EventResponse?> UpdateAsync(
        int id,
        UpdateEventRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
