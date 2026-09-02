using EventParking.API.Data;
using EventParking.API.DTOs.Events;
using EventParking.API.Entities;
using EventParking.API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Services;

public sealed class EventService : IEventService
{
    private readonly AppDbContext _dbContext;

    public EventService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EventResponse>> GetAllAsync(
        string? search = null,
        DateTime? dateFromUtc = null,
        DateTime? dateToUtc = null,
        int? venueId = null,
        int? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query =
            from eventEntity in _dbContext.Events.AsNoTracking()
            join venue in _dbContext.Venues.AsNoTracking()
                on eventEntity.VenueId equals venue.Id
            join category in _dbContext.EventCategories.AsNoTracking()
                on eventEntity.EventCategoryId equals category.Id
            select new EventResponse
            {
                Id = eventEntity.Id,
                Name = eventEntity.Name,
                Description = eventEntity.Description,
                VenueId = eventEntity.VenueId,
                VenueName = venue.Name,
                EventCategoryId = eventEntity.EventCategoryId,
                CategoryName = category.Name,
                StartDateTimeUtc = eventEntity.StartDateTimeUtc,
                EndDateTimeUtc = eventEntity.EndDateTimeUtc,
                TicketPrice = eventEntity.TicketPrice,
                ParkingFee = eventEntity.ParkingFee,
                Capacity = eventEntity.Capacity,
                CreatedAtUtc = eventEntity.CreatedAtUtc,
                UpdatedAtUtc = eventEntity.UpdatedAtUtc
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(eventResponse =>
                eventResponse.Name.Contains(searchTerm) ||
                (eventResponse.Description != null &&
                 eventResponse.Description.Contains(searchTerm)));
        }

        if (dateFromUtc.HasValue)
        {
            query = query.Where(eventResponse =>
                eventResponse.StartDateTimeUtc >= dateFromUtc.Value);
        }

        if (dateToUtc.HasValue)
        {
            query = query.Where(eventResponse =>
                eventResponse.StartDateTimeUtc <= dateToUtc.Value);
        }

        if (venueId.HasValue)
        {
            query = query.Where(eventResponse =>
                eventResponse.VenueId == venueId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(eventResponse =>
                eventResponse.EventCategoryId == categoryId.Value);
        }

        return await query
            .OrderBy(eventResponse => eventResponse.StartDateTimeUtc)
            .ThenBy(eventResponse => eventResponse.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<EventResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await (
            from eventEntity in _dbContext.Events.AsNoTracking()
            join venue in _dbContext.Venues.AsNoTracking()
                on eventEntity.VenueId equals venue.Id
            join category in _dbContext.EventCategories.AsNoTracking()
                on eventEntity.EventCategoryId equals category.Id
            where eventEntity.Id == id
            select new EventResponse
            {
                Id = eventEntity.Id,
                Name = eventEntity.Name,
                Description = eventEntity.Description,
                VenueId = eventEntity.VenueId,
                VenueName = venue.Name,
                EventCategoryId = eventEntity.EventCategoryId,
                CategoryName = category.Name,
                StartDateTimeUtc = eventEntity.StartDateTimeUtc,
                EndDateTimeUtc = eventEntity.EndDateTimeUtc,
                TicketPrice = eventEntity.TicketPrice,
                ParkingFee = eventEntity.ParkingFee,
                Capacity = eventEntity.Capacity,
                CreatedAtUtc = eventEntity.CreatedAtUtc,
                UpdatedAtUtc = eventEntity.UpdatedAtUtc
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<EventResponse> CreateAsync(
        CreateEventRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateAsync(
            request.VenueId,
            request.EventCategoryId,
            request.StartDateTimeUtc,
            request.EndDateTimeUtc,
            request.Capacity,
            null,
            cancellationToken);

        var now = DateTime.UtcNow;

        var eventEntity = new Event
        {
            Name = request.Name.Trim(),
            Description = NormalizeDescription(request.Description),
            VenueId = request.VenueId,
            EventCategoryId = request.EventCategoryId,
            StartDateTimeUtc = request.StartDateTimeUtc,
            EndDateTimeUtc = request.EndDateTimeUtc,
            TicketPrice = request.TicketPrice,
            ParkingFee = request.ParkingFee,
            Capacity = request.Capacity,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _dbContext.Events.Add(eventEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(eventEntity.Id, cancellationToken))!;
    }

    public async Task<EventResponse?> UpdateAsync(
        int id,
        UpdateEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var eventEntity = await _dbContext.Events
            .SingleOrDefaultAsync(
                eventItem => eventItem.Id == id,
                cancellationToken);

        if (eventEntity is null)
        {
            return null;
        }

        await ValidateAsync(
            request.VenueId,
            request.EventCategoryId,
            request.StartDateTimeUtc,
            request.EndDateTimeUtc,
            request.Capacity,
            id,
            cancellationToken);

        eventEntity.Name = request.Name.Trim();
        eventEntity.Description = NormalizeDescription(request.Description);
        eventEntity.VenueId = request.VenueId;
        eventEntity.EventCategoryId = request.EventCategoryId;
        eventEntity.StartDateTimeUtc = request.StartDateTimeUtc;
        eventEntity.EndDateTimeUtc = request.EndDateTimeUtc;
        eventEntity.TicketPrice = request.TicketPrice;
        eventEntity.ParkingFee = request.ParkingFee;
        eventEntity.Capacity = request.Capacity;
        eventEntity.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var eventEntity = await _dbContext.Events
            .SingleOrDefaultAsync(
                eventItem => eventItem.Id == id,
                cancellationToken);

        if (eventEntity is null)
        {
            return false;
        }

        _dbContext.Events.Remove(eventEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ValidateAsync(
        int venueId,
        int eventCategoryId,
        DateTime startDateTimeUtc,
        DateTime endDateTimeUtc,
        int capacity,
        int? existingEventId,
        CancellationToken cancellationToken)
    {
        if (endDateTimeUtc <= startDateTimeUtc)
        {
            throw new ArgumentException(
                "Event end date/time must be after the start date/time.");
        }

        var venueCapacity = await _dbContext.Venues
            .AsNoTracking()
            .Where(venue => venue.Id == venueId)
            .Select(venue => (int?)venue.Capacity)
            .SingleOrDefaultAsync(cancellationToken);

        if (!venueCapacity.HasValue)
        {
            throw new ArgumentException(
                "The selected venue does not exist.");
        }

        if (capacity > venueCapacity.Value)
        {
            throw new ArgumentException(
                "Event capacity cannot exceed the selected venue capacity.");
        }

        var categoryExists = await _dbContext.EventCategories
            .AsNoTracking()
            .AnyAsync(
                category => category.Id == eventCategoryId,
                cancellationToken);

        if (!categoryExists)
        {
            throw new ArgumentException(
                "The selected event category does not exist.");
        }

        var venueHasOverlap = await _dbContext.Events
            .AsNoTracking()
            .AnyAsync(
                eventItem =>
                    eventItem.VenueId == venueId &&
                    (!existingEventId.HasValue ||
                     eventItem.Id != existingEventId.Value) &&
                    startDateTimeUtc < eventItem.EndDateTimeUtc &&
                    eventItem.StartDateTimeUtc < endDateTimeUtc,
                cancellationToken);

        if (venueHasOverlap)
        {
            throw new InvalidOperationException(
                "The selected venue already has an overlapping event.");
        }
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }
}
