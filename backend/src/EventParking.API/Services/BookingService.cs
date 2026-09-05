using EventParking.API.Data;
using EventParking.API.DTOs.Bookings;
using EventParking.API.Entities;
using EventParking.API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Services;

public sealed class BookingService : IBookingService
{
    private const string AvailableStatus = "Available";
    private const string BookedStatus = "Booked";
    private const string HeldStatus = "Held";
    private const string PendingPaymentStatus = "Pending";

    private readonly AppDbContext _dbContext;

    public BookingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BookingResponse> CreateAsync(
        CreateBookingRequest request,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var eventExists = await _dbContext.Events
            .AnyAsync(
                e => e.Id == request.EventId,
                cancellationToken);

        if (!eventExists)
        {
            throw new ArgumentException(
                "Event does not exist.");
        }

        var duplicateBooking = await _dbContext.Bookings
            .AnyAsync(
                b =>
                    b.CustomerId == customerId &&
                    b.EventId == request.EventId &&
                    b.Status != "Cancelled",
                cancellationToken);

        if (duplicateBooking)
        {
            throw new InvalidOperationException(
                "Customer already has a booking for this event.");
        }

        if (!request.SeatId.HasValue)
        {
            throw new InvalidOperationException(
                "At least one seat is required.");
        }

        if (request.SeatId.HasValue)
        {
            var seat = await _dbContext.Seats
                .FirstOrDefaultAsync(
                    s => s.Id == request.SeatId.Value &&
                         s.EventId == request.EventId,
                    cancellationToken);

            if (seat is null)
            {
                throw new ArgumentException(
                    "Seat does not exist.");
            }

            if (seat.Status != AvailableStatus)
            {
                throw new InvalidOperationException(
                    "Seat is not available.");
            }

            seat.Status = BookedStatus;
        }

        if (request.ParkingSlotId.HasValue)
        {
            var slot = await _dbContext.ParkingSlots
                .FirstOrDefaultAsync(
                    p => p.Id == request.ParkingSlotId.Value &&
                         p.EventId == request.EventId,
                    cancellationToken);

            if (slot is null)
            {
                throw new ArgumentException(
                    "Parking slot does not exist.");
            }

            if (slot.Status != AvailableStatus)
            {
                throw new InvalidOperationException(
                    "Parking slot is not available.");
            }

            slot.Status = BookedStatus;
        }

        var booking = new Booking
        {
            BookingNumber = $"BK-{DateTime.UtcNow:yyyyMMddHHmmss}",
            EventId = request.EventId,
            SeatId = request.SeatId,
            ParkingSlotId = request.ParkingSlotId,
            CustomerId = customerId,
            Status = HeldStatus,
            PaymentStatus = PendingPaymentStatus
        };

        _dbContext.Bookings.Add(booking);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new BookingResponse
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            EventId = booking.EventId,
            SeatId = booking.SeatId,
            ParkingSlotId = booking.ParkingSlotId,
            Status = booking.Status,
            PaymentStatus = booking.PaymentStatus
        };
    }
}










