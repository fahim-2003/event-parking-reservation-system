using EventParking.API.Configurations;
using EventParking.API.Data;
using EventParking.API.DTOs.Bookings;
using EventParking.API.Entities;
using EventParking.API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventParking.API.Services;

public sealed class BookingService : IBookingService
{
    private const string AvailableStatus = "Available";
    private const string HeldStatus = "Held";
    private const string ConfirmedStatus = "Confirmed";
    private const string CancelledStatus = "Cancelled";
    private const string ExpiredStatus = "Expired";
    private const string PendingPaymentStatus = "Pending";

    private readonly AppDbContext _dbContext;
    private readonly BookingSettings _settings;

    public BookingService(
        AppDbContext dbContext,
        IOptions<BookingSettings> settings)
    {
        _dbContext = dbContext;
        _settings = settings.Value;
    }

    public async Task<BookingResponse> CreateAsync(
        CreateBookingRequest request,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        if (request.SeatIds is null || request.SeatIds.Count == 0)
        {
            throw new ArgumentException(
                "At least one seat is required.");
        }

        var seatIds = request.SeatIds
            .Distinct()
            .ToList();

        if (seatIds.Count != request.SeatIds.Count)
        {
            throw new ArgumentException(
                "Duplicate seat selections are not allowed.");
        }

        var eventExists = await _dbContext.Events
            .AnyAsync(
                eventEntity => eventEntity.Id == request.EventId,
                cancellationToken);

        if (!eventExists)
        {
            throw new ArgumentException(
                "Event does not exist.");
        }

        await using var transaction =
            _dbContext.Database.IsRelational()
                ? await _dbContext.Database.BeginTransactionAsync(
                    cancellationToken)
                : null;

        var seats = await _dbContext.Seats
            .Where(
                seat =>
                    seat.EventId == request.EventId &&
                    seatIds.Contains(seat.Id))
            .ToListAsync(cancellationToken);

        if (seats.Count != seatIds.Count)
        {
            throw new ArgumentException(
                "One or more selected seats do not belong to this event.");
        }

        if (seats.Any(seat => seat.Status != AvailableStatus))
        {
            throw new InvalidOperationException(
                "One or more selected seats are no longer available.");
        }

        ParkingSlot? parkingSlot = null;

        if (request.ParkingSlotId.HasValue)
        {
            parkingSlot = await _dbContext.ParkingSlots
                .FirstOrDefaultAsync(
                    slot =>
                        slot.Id == request.ParkingSlotId.Value &&
                        slot.EventId == request.EventId,
                    cancellationToken);

            if (parkingSlot is null)
            {
                throw new ArgumentException(
                    "Parking slot does not belong to this event.");
            }

            if (parkingSlot.Status != AvailableStatus)
            {
                throw new InvalidOperationException(
                    "Selected parking slot is no longer available.");
            }
        }

        foreach (var seat in seats)
        {
            seat.Status = HeldStatus;
        }

        if (parkingSlot is not null)
        {
            parkingSlot.Status = HeldStatus;
        }

        var booking = new Booking
        {
            BookingNumber = CreateBookingNumber(),
            EventId = request.EventId,
            CustomerId = customerId,
            Status = HeldStatus,
            PaymentStatus = PendingPaymentStatus,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var seat in seats)
        {
            booking.BookingSeats.Add(
                new BookingSeat
                {
                    Seat = seat
                });
        }

        if (parkingSlot is not null)
        {
            booking.ParkingReservation =
                new ParkingReservation
                {
                    ParkingSlot = parkingSlot,
                    CreatedAt = DateTime.UtcNow
                };
        }

        _dbContext.Bookings.Add(booking);

        _dbContext.Notifications.Add(
            new Notification
            {
                UserId = customerId,
                Message =
                    $"Booking {booking.BookingNumber} was created and is being held for payment.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw new InvalidOperationException(
                "One or more selected resources were reserved by another customer.");
        }
        catch (DbUpdateException)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw new InvalidOperationException(
                "The booking could not be created because a selected resource is no longer available.");
        }

        return Map(booking);
    }

    public async Task<IReadOnlyList<BookingResponse>> GetForCustomerAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var bookings = await BookingQuery()
            .AsNoTracking()
            .Where(booking => booking.CustomerId == customerId)
            .OrderByDescending(booking => booking.CreatedAt)
            .ToListAsync(cancellationToken);

        return bookings
            .Select(Map)
            .ToList();
    }

    public async Task<BookingResponse?> GetByIdAsync(
        int bookingId,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var booking = await BookingQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item =>
                    item.Id == bookingId &&
                    item.CustomerId == customerId,
                cancellationToken);

        return booking is null
            ? null
            : Map(booking);
    }

    public async Task<BookingResponse> CancelAsync(
        int bookingId,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var booking = await BookingQuery()
            .FirstOrDefaultAsync(
                item =>
                    item.Id == bookingId &&
                    item.CustomerId == customerId,
                cancellationToken);

        if (booking is null)
        {
            throw new KeyNotFoundException(
                "Booking does not exist.");
        }

        if (booking.Status == CancelledStatus)
        {
            throw new InvalidOperationException(
                "Booking is already cancelled.");
        }

        if (booking.Status == ExpiredStatus)
        {
            throw new InvalidOperationException(
                "Expired bookings cannot be cancelled.");
        }

        foreach (var bookingSeat in booking.BookingSeats)
        {
            bookingSeat.Seat.Status = AvailableStatus;
        }

        if (booking.ParkingReservation is not null)
        {
            booking.ParkingReservation.ParkingSlot.Status =
                AvailableStatus;
        }

        booking.Status = CancelledStatus;
        booking.CancelledAt = DateTime.UtcNow;

        _dbContext.Notifications.Add(
            new Notification
            {
                UserId = customerId,
                Message =
                    $"Booking {booking.BookingNumber} was cancelled.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "Booking state changed while cancellation was being processed.");
        }

        return Map(booking);
    }

    public async Task<int> ExpireHeldBookingsAsync(
        CancellationToken cancellationToken = default)
    {
        var cutoff =
            DateTime.UtcNow.AddMinutes(-_settings.HoldMinutes);

        var bookings = await BookingQuery()
            .Where(
                booking =>
                    booking.Status == HeldStatus &&
                    booking.CreatedAt <= cutoff)
            .ToListAsync(cancellationToken);

        if (bookings.Count == 0)
        {
            return 0;
        }

        foreach (var booking in bookings)
        {
            booking.Status = ExpiredStatus;

            foreach (var bookingSeat in booking.BookingSeats)
            {
                if (bookingSeat.Seat.Status == HeldStatus)
                {
                    bookingSeat.Seat.Status = AvailableStatus;
                }
            }

            if (booking.ParkingReservation is not null &&
                booking.ParkingReservation.ParkingSlot.Status ==
                HeldStatus)
            {
                booking.ParkingReservation.ParkingSlot.Status =
                    AvailableStatus;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return bookings.Count;
    }

    private IQueryable<Booking> BookingQuery()
    {
        return _dbContext.Bookings
            .Include(booking => booking.BookingSeats)
                .ThenInclude(bookingSeat => bookingSeat.Seat)
            .Include(booking => booking.ParkingReservation)
                .ThenInclude(reservation => reservation!.ParkingSlot);
    }

    private BookingResponse Map(Booking booking)
    {
        return new BookingResponse
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            EventId = booking.EventId,
            SeatIds = booking.BookingSeats
                .Select(bookingSeat => bookingSeat.SeatId)
                .OrderBy(seatId => seatId)
                .ToList(),
            ParkingSlotId =
                booking.ParkingReservation?.ParkingSlotId,
            Status = booking.Status,
            PaymentStatus = booking.PaymentStatus,
            CreatedAt = booking.CreatedAt,
            HoldExpiresAt =
                booking.CreatedAt.AddMinutes(_settings.HoldMinutes)
        };
    }

    private static string CreateBookingNumber()
    {
        var suffix = Guid.NewGuid()
            .ToString("N")[..6]
            .ToUpperInvariant();

        return $"BK-{DateTime.UtcNow:yyyyMMddHHmmss}-{suffix}";
    }
}

