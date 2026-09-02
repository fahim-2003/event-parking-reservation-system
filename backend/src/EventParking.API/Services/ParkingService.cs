using EventParking.API.Data;
using EventParking.API.DTOs.Parking;
using EventParking.API.Entities;
using EventParking.API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Services;

public sealed class ParkingService : IParkingService
{
    private const string AvailableStatus = "Available";

    private readonly AppDbContext _dbContext;

    public ParkingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ParkingSlotResponse>> GetByEventAsync(
        int eventId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ParkingSlots
            .AsNoTracking()
            .Where(slot => slot.EventId == eventId)
            .OrderBy(slot => slot.Zone)
            .ThenBy(slot => slot.SlotNumber)
            .Select(slot => new ParkingSlotResponse
            {
                Id = slot.Id,
                EventId = slot.EventId,
                Zone = slot.Zone,
                SlotNumber = slot.SlotNumber,
                Status = slot.Status,
                RowVersion = slot.RowVersion
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ParkingSlotResponse>> GenerateAsync(
        int eventId,
        GenerateParkingLayoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var eventExists = await _dbContext.Events
            .AsNoTracking()
            .AnyAsync(
                eventEntity => eventEntity.Id == eventId,
                cancellationToken);

        if (!eventExists)
        {
            throw new ArgumentException(
                "The selected event does not exist.");
        }

        if (request.NumberOfSlots <= 0)
        {
            throw new ArgumentException(
                "Number of parking slots must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(request.Zone))
        {
            throw new ArgumentException(
                "Parking zone is required.");
        }

        var normalizedZone = request.Zone
            .Trim()
            .ToUpperInvariant();

        var zoneAlreadyExists = await _dbContext.ParkingSlots
            .AsNoTracking()
            .AnyAsync(
                slot =>
                    slot.EventId == eventId &&
                    slot.Zone == normalizedZone,
                cancellationToken);

        if (zoneAlreadyExists)
        {
            throw new InvalidOperationException(
                "A parking layout already exists for this zone.");
        }

        var parkingSlots = new List<ParkingSlot>(
            request.NumberOfSlots);

        for (var slotNumber = 1;
             slotNumber <= request.NumberOfSlots;
             slotNumber++)
        {
            parkingSlots.Add(new ParkingSlot
            {
                EventId = eventId,
                Zone = normalizedZone,
                SlotNumber = slotNumber,
                Status = AvailableStatus
            });
        }

        _dbContext.ParkingSlots.AddRange(parkingSlots);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return parkingSlots
            .OrderBy(slot => slot.SlotNumber)
            .Select(ToResponse)
            .ToList();
    }

    public async Task<ParkingSlotResponse?> UpdateAsync(
        int eventId,
        int parkingSlotId,
        UpdateParkingSlotRequest request,
        CancellationToken cancellationToken = default)
    {
        var parkingSlot = await _dbContext.ParkingSlots
            .SingleOrDefaultAsync(
                slot =>
                    slot.Id == parkingSlotId &&
                    slot.EventId == eventId,
                cancellationToken);

        if (parkingSlot is null)
        {
            return null;
        }

        if (!string.Equals(
                parkingSlot.Status,
                AvailableStatus,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Held or occupied parking slots cannot be renamed.");
        }

        if (request.RowVersion.Length == 0)
        {
            throw new ArgumentException(
                "RowVersion is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Zone))
        {
            throw new ArgumentException(
                "Parking zone is required.");
        }

        var normalizedZone = request.Zone
            .Trim()
            .ToUpperInvariant();

        var duplicateExists = await _dbContext.ParkingSlots
            .AsNoTracking()
            .AnyAsync(
                slot =>
                    slot.EventId == eventId &&
                    slot.Id != parkingSlotId &&
                    slot.Zone == normalizedZone &&
                    slot.SlotNumber == request.SlotNumber,
                cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                "Another parking slot already uses that zone and slot number.");
        }

        _dbContext.Entry(parkingSlot)
            .Property(slot => slot.RowVersion)
            .OriginalValue = request.RowVersion;

        parkingSlot.Zone = normalizedZone;
        parkingSlot.SlotNumber = request.SlotNumber;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(parkingSlot);
    }

    public async Task<bool> DeleteAsync(
        int eventId,
        int parkingSlotId,
        byte[] rowVersion,
        CancellationToken cancellationToken = default)
    {
        var parkingSlot = await _dbContext.ParkingSlots
            .SingleOrDefaultAsync(
                slot =>
                    slot.Id == parkingSlotId &&
                    slot.EventId == eventId,
                cancellationToken);

        if (parkingSlot is null)
        {
            return false;
        }

        if (!string.Equals(
                parkingSlot.Status,
                AvailableStatus,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Held or occupied parking slots cannot be deleted.");
        }

        if (rowVersion.Length == 0)
        {
            throw new ArgumentException(
                "RowVersion is required.");
        }

        _dbContext.Entry(parkingSlot)
            .Property(slot => slot.RowVersion)
            .OriginalValue = rowVersion;

        _dbContext.ParkingSlots.Remove(parkingSlot);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static ParkingSlotResponse ToResponse(
        ParkingSlot parkingSlot)
    {
        return new ParkingSlotResponse
        {
            Id = parkingSlot.Id,
            EventId = parkingSlot.EventId,
            Zone = parkingSlot.Zone,
            SlotNumber = parkingSlot.SlotNumber,
            Status = parkingSlot.Status,
            RowVersion = parkingSlot.RowVersion
        };
    }
}
