using EventParking.API.Data;
using EventParking.API.DTOs.Seats;
using EventParking.API.Entities;
using EventParking.API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Services;

public sealed class SeatService : ISeatService
{
    private const string AvailableStatus = "Available";

    private readonly AppDbContext _dbContext;

    public SeatService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SeatResponse>> GetByEventAsync(
        int eventId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Seats
            .AsNoTracking()
            .Where(seat => seat.EventId == eventId)
            .OrderBy(seat => seat.RowLabel)
            .ThenBy(seat => seat.SeatNumber)
            .Select(seat => new SeatResponse
            {
                Id = seat.Id,
                EventId = seat.EventId,
                RowLabel = seat.RowLabel,
                SeatNumber = seat.SeatNumber,
                DisplayLabel = seat.DisplayLabel,
                Status = seat.Status,
                RowVersion = seat.RowVersion
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SeatResponse>> GenerateAsync(
        int eventId,
        GenerateSeatMapRequest request,
        CancellationToken cancellationToken = default)
    {
        var eventCapacity = await _dbContext.Events
            .AsNoTracking()
            .Where(eventEntity => eventEntity.Id == eventId)
            .Select(eventEntity => (int?)eventEntity.Capacity)
            .SingleOrDefaultAsync(cancellationToken);

        if (!eventCapacity.HasValue)
        {
            throw new ArgumentException("The selected event does not exist.");
        }

        if (request.Rows <= 0 || request.SeatsPerRow <= 0)
        {
            throw new ArgumentException(
                "Rows and seats per row must both be greater than 0.");
        }

        var requestedSeatCount =
            (long)request.Rows * request.SeatsPerRow;

        if (requestedSeatCount != eventCapacity.Value)
        {
            throw new ArgumentException(
                $"Seat map must contain exactly {eventCapacity.Value} seats to match the event capacity.");
        }

        var layoutExists = await _dbContext.Seats
            .AsNoTracking()
            .AnyAsync(
                seat => seat.EventId == eventId,
                cancellationToken);

        if (layoutExists)
        {
            throw new InvalidOperationException(
                "A seat map already exists for this event.");
        }

        var seats = new List<Seat>(eventCapacity.Value);

        for (var rowIndex = 0; rowIndex < request.Rows; rowIndex++)
        {
            var rowLabel = ToRowLabel(rowIndex);

            for (var seatNumber = 1;
                 seatNumber <= request.SeatsPerRow;
                 seatNumber++)
            {
                seats.Add(new Seat
                {
                    EventId = eventId,
                    RowLabel = rowLabel,
                    SeatNumber = seatNumber,
                    DisplayLabel = $"{rowLabel}{seatNumber}",
                    Status = AvailableStatus
                });
            }
        }

        _dbContext.Seats.AddRange(seats);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return seats
            .OrderBy(seat => seat.RowLabel)
            .ThenBy(seat => seat.SeatNumber)
            .Select(ToResponse)
            .ToList();
    }

    public async Task<SeatResponse?> UpdateAsync(
        int eventId,
        int seatId,
        UpdateSeatRequest request,
        CancellationToken cancellationToken = default)
    {
        var seat = await _dbContext.Seats
            .SingleOrDefaultAsync(
                seatEntity =>
                    seatEntity.Id == seatId &&
                    seatEntity.EventId == eventId,
                cancellationToken);

        if (seat is null)
        {
            return null;
        }

        if (!string.Equals(
                seat.Status,
                AvailableStatus,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Held or booked seats cannot be renamed.");
        }

        if (request.RowVersion.Length == 0)
        {
            throw new ArgumentException("RowVersion is required.");
        }

        var normalizedRowLabel = request.RowLabel
            .Trim()
            .ToUpperInvariant();

        var displayLabel =
            $"{normalizedRowLabel}{request.SeatNumber}";

        var duplicateExists = await _dbContext.Seats
            .AsNoTracking()
            .AnyAsync(
                otherSeat =>
                    otherSeat.EventId == eventId &&
                    otherSeat.Id != seatId &&
                    otherSeat.DisplayLabel == displayLabel,
                cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                "Another seat already uses that display label for this event.");
        }

        _dbContext.Entry(seat)
            .Property(seatEntity => seatEntity.RowVersion)
            .OriginalValue = request.RowVersion;

        seat.RowLabel = normalizedRowLabel;
        seat.SeatNumber = request.SeatNumber;
        seat.DisplayLabel = displayLabel;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(seat);
    }

    public async Task<bool> DeleteAsync(
        int eventId,
        int seatId,
        byte[] rowVersion,
        CancellationToken cancellationToken = default)
    {
        var seat = await _dbContext.Seats
            .SingleOrDefaultAsync(
                seatEntity =>
                    seatEntity.Id == seatId &&
                    seatEntity.EventId == eventId,
                cancellationToken);

        if (seat is null)
        {
            return false;
        }

        if (!string.Equals(
                seat.Status,
                AvailableStatus,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Held or booked seats cannot be deleted.");
        }

        if (rowVersion.Length == 0)
        {
            throw new ArgumentException("RowVersion is required.");
        }

        _dbContext.Entry(seat)
            .Property(seatEntity => seatEntity.RowVersion)
            .OriginalValue = rowVersion;

        _dbContext.Seats.Remove(seat);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static SeatResponse ToResponse(Seat seat)
    {
        return new SeatResponse
        {
            Id = seat.Id,
            EventId = seat.EventId,
            RowLabel = seat.RowLabel,
            SeatNumber = seat.SeatNumber,
            DisplayLabel = seat.DisplayLabel,
            Status = seat.Status,
            RowVersion = seat.RowVersion
        };
    }

    private static string ToRowLabel(int zeroBasedRowIndex)
    {
        var rowNumber = zeroBasedRowIndex + 1;
        var label = string.Empty;

        while (rowNumber > 0)
        {
            rowNumber--;

            label =
                (char)('A' + rowNumber % 26) +
                label;

            rowNumber /= 26;
        }

        return label;
    }
}

