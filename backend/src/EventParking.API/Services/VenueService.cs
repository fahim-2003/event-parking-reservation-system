using EventParking.API.Data;
using EventParking.API.DTOs.Venues;
using EventParking.API.Entities;
using EventParking.API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Services;

public sealed class VenueService : IVenueService
{
    private readonly AppDbContext _dbContext;

    public VenueService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<VenueResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Venues
            .AsNoTracking()
            .OrderBy(venue => venue.Name)
            .Select(venue => ToResponse(venue))
            .ToListAsync(cancellationToken);
    }

    public async Task<VenueResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Venues
            .AsNoTracking()
            .Where(venue => venue.Id == id)
            .Select(venue => ToResponse(venue))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<VenueResponse> CreateAsync(
        CreateVenueRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var venue = new Venue
        {
            Name = request.Name.Trim(),
            Address = request.Address.Trim(),
            Capacity = request.Capacity,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _dbContext.Venues.Add(venue);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(venue);
    }

    public async Task<VenueResponse?> UpdateAsync(
        int id,
        UpdateVenueRequest request,
        CancellationToken cancellationToken = default)
    {
        var venue = await _dbContext.Venues
            .SingleOrDefaultAsync(
                venue => venue.Id == id,
                cancellationToken);

        if (venue is null)
        {
            return null;
        }

        venue.Name = request.Name.Trim();
        venue.Address = request.Address.Trim();
        venue.Capacity = request.Capacity;
        venue.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(venue);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var venue = await _dbContext.Venues
            .SingleOrDefaultAsync(
                venue => venue.Id == id,
                cancellationToken);

        if (venue is null)
        {
            return false;
        }

        _dbContext.Venues.Remove(venue);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static VenueResponse ToResponse(Venue venue)
    {
        return new VenueResponse
        {
            Id = venue.Id,
            Name = venue.Name,
            Address = venue.Address,
            Capacity = venue.Capacity,
            CreatedAtUtc = venue.CreatedAtUtc,
            UpdatedAtUtc = venue.UpdatedAtUtc
        };
    }
}