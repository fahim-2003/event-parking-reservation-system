using EventParking.API.Data;
using EventParking.API.DTOs.Payments;
using EventParking.API.Entities;
using EventParking.API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using EventParking.API.Configurations;

namespace EventParking.API.Services;

public sealed class PaymentService : IPaymentService
{
    private const string PendingStatus = "Pending";
    private const string CompletedStatus = "Completed";
    private const string HeldBookingStatus = "Held";
    private const string ConfirmedBookingStatus = "Confirmed";
    private const string BookedSeatStatus = "Booked";
    private const string OccupiedParkingStatus = "Occupied";

    private readonly AppDbContext _dbContext;
    private readonly BookingSettings _bookingSettings;

    public PaymentService(
        AppDbContext dbContext,
        IOptions<BookingSettings>? bookingSettings = null)
    {
        _dbContext = dbContext;
        _bookingSettings =
            bookingSettings?.Value ?? new BookingSettings();
    }

    // Retained temporarily for existing tests/internal compatibility.
    public async Task<PaymentResponse> CreateAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var bookingExists = await _dbContext.Bookings
            .AnyAsync(
                booking => booking.Id == request.BookingId,
                cancellationToken);

        if (!bookingExists)
        {
            throw new ArgumentException(
                "Booking does not exist.");
        }

        var completedExists = await _dbContext.Payments
            .AnyAsync(
                payment =>
                    payment.BookingId == request.BookingId &&
                    payment.Status == CompletedStatus,
                cancellationToken);

        if (completedExists)
        {
            throw new InvalidOperationException(
                "Payment already completed.");
        }

        var payment = new Payment
        {
            BookingId = request.BookingId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Status = PendingStatus,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Payments.Add(payment);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(payment);
    }

    public async Task<PaymentResponse> ProcessAsync(
        ProcessPaymentRequest request,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.PaymentMethod))
        {
            throw new ArgumentException(
                "Payment method is required.");
        }

        await using var transaction =
            _dbContext.Database.IsRelational()
                ? await _dbContext.Database.BeginTransactionAsync(
                    cancellationToken)
                : null;

        var booking = await _dbContext.Bookings
            .Include(item => item.BookingSeats)
                .ThenInclude(bookingSeat => bookingSeat.Seat)
            .Include(item => item.ParkingReservation)
                .ThenInclude(reservation => reservation!.ParkingSlot)
            .FirstOrDefaultAsync(
                item =>
                    item.Id == request.BookingId &&
                    item.CustomerId == customerId,
                cancellationToken);

        if (booking is null)
        {
            throw new ArgumentException(
                "Booking does not exist.");
        }

        if (booking.Status != HeldBookingStatus)
        {
            throw new InvalidOperationException(
                "Only held bookings can be paid.");
        }

        var holdExpiresAt =
            booking.CreatedAt.AddMinutes(
                _bookingSettings.HoldMinutes);

        if (holdExpiresAt <= DateTime.UtcNow)
        {
            booking.Status = "Expired";

            foreach (var bookingSeat in booking.BookingSeats)
            {
                if (bookingSeat.Seat.Status == HeldBookingStatus)
                {
                    bookingSeat.Seat.Status = "Available";
                }
            }

            if (booking.ParkingReservation is not null &&
                booking.ParkingReservation.ParkingSlot.Status ==
                HeldBookingStatus)
            {
                booking.ParkingReservation.ParkingSlot.Status =
                    "Available";
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            throw new InvalidOperationException(
                "Booking hold has expired.");
        }

        if (booking.BookingSeats.Count == 0)
        {
            throw new InvalidOperationException(
                "Booking must contain at least one seat.");
        }

        var completedExists = await _dbContext.Payments
            .AnyAsync(
                payment =>
                    payment.BookingId == booking.Id &&
                    payment.Status == CompletedStatus,
                cancellationToken);

        if (completedExists)
        {
            throw new InvalidOperationException(
                "Payment already completed.");
        }

        var eventEntity = await _dbContext.Events
            .FirstOrDefaultAsync(
                item => item.Id == booking.EventId,
                cancellationToken);

        if (eventEntity is null)
        {
            throw new ArgumentException(
                "Event does not exist.");
        }

        var amount =
            eventEntity.TicketPrice *
            booking.BookingSeats.Count;

        if (booking.ParkingReservation is not null)
        {
            amount += eventEntity.ParkingFee;
        }

        var payment = new Payment
        {
            BookingId = booking.Id,
            Amount = amount,
            PaymentMethod = request.PaymentMethod.Trim(),
            Status = CompletedStatus,
            CreatedAt = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow
        };

        _dbContext.Payments.Add(payment);

        booking.PaymentStatus = CompletedStatus;
        booking.Status = ConfirmedBookingStatus;
        booking.ConfirmedAt = DateTime.UtcNow;

        foreach (var bookingSeat in booking.BookingSeats)
        {
            if (bookingSeat.Seat.Status != HeldBookingStatus)
            {
                throw new InvalidOperationException(
                    "One or more booking seats are no longer held.");
            }

            bookingSeat.Seat.Status = BookedSeatStatus;
        }

        if (booking.ParkingReservation is not null)
        {
            var parkingSlot =
                booking.ParkingReservation.ParkingSlot;

            if (parkingSlot.Status != HeldBookingStatus)
            {
                throw new InvalidOperationException(
                    "Reserved parking is no longer held.");
            }

            parkingSlot.Status = OccupiedParkingStatus;
        }

        _dbContext.Notifications.Add(
            new Notification
            {
                UserId = customerId,
                Message =
                    $"Payment completed for booking {booking.BookingNumber}.",
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
                "Booking state changed while payment was being processed.");
        }
        catch (DbUpdateException)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw new InvalidOperationException(
                "Payment could not be completed.");
        }

        return Map(payment);
    }

    public async Task<IReadOnlyList<PaymentResponse>> GetForCustomerAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .AsNoTracking()
            .Where(
                payment =>
                    _dbContext.Bookings.Any(
                        booking =>
                            booking.Id == payment.BookingId &&
                            booking.CustomerId == customerId))
            .OrderByDescending(payment => payment.CreatedAt)
            .Select(
                payment => new PaymentResponse
                {
                    Id = payment.Id,
                    BookingId = payment.BookingId,
                    Amount = payment.Amount,
                    PaymentMethod = payment.PaymentMethod,
                    Status = payment.Status,
                    CreatedAt = payment.CreatedAt
                })
            .ToListAsync(cancellationToken);
    }

    private static PaymentResponse Map(Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id,
            BookingId = payment.BookingId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status,
            CreatedAt = payment.CreatedAt
        };
    }
}
