using EventParking.API.Configurations;
using EventParking.API.Data;
using EventParking.API.DTOs.Bookings;
using EventParking.API.DTOs.Payments;
using EventParking.API.Entities;
using EventParking.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventParking.Tests.Services;

public sealed class BookingV2RegressionTests
{
    [Fact]
    public async Task CreateAsync_WithMultipleSeats_HoldsAllSeats()
    {
        await using var db = CreateDbContext();

        var eventEntity = CreateEvent();
        db.Events.Add(eventEntity);
        await db.SaveChangesAsync();

        var seat1 = CreateSeat(eventEntity.Id, 1, "A1");
        var seat2 = CreateSeat(eventEntity.Id, 2, "A2");

        db.Seats.AddRange(seat1, seat2);
        await db.SaveChangesAsync();

        var service = CreateBookingService(db);

        var result = await service.CreateAsync(
            new CreateBookingRequest
            {
                EventId = eventEntity.Id,
                SeatIds = [seat1.Id, seat2.Id]
            },
            "customer-1");

        Assert.Equal("Held", result.Status);
        Assert.Equal(2, result.SeatIds.Count);

        var seats = await db.Seats
            .OrderBy(seat => seat.Id)
            .ToListAsync();

        Assert.All(
            seats,
            seat => Assert.Equal("Held", seat.Status));

        Assert.Equal(
            2,
            await db.BookingSeats.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateSeatIds_RejectsRequest()
    {
        await using var db = CreateDbContext();

        var eventEntity = CreateEvent();
        db.Events.Add(eventEntity);
        await db.SaveChangesAsync();

        var seat = CreateSeat(eventEntity.Id, 1, "A1");
        db.Seats.Add(seat);
        await db.SaveChangesAsync();

        var service = CreateBookingService(db);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                new CreateBookingRequest
                {
                    EventId = eventEntity.Id,
                    SeatIds = [seat.Id, seat.Id]
                },
                "customer-1"));
    }

    [Fact]
    public async Task CreateAsync_WithSeatFromAnotherEvent_RejectsRequest()
    {
        await using var db = CreateDbContext();

        var event1 = CreateEvent("Event One");
        var event2 = CreateEvent("Event Two");

        db.Events.AddRange(event1, event2);
        await db.SaveChangesAsync();

        var seat = CreateSeat(event2.Id, 1, "B1");
        db.Seats.Add(seat);
        await db.SaveChangesAsync();

        var service = CreateBookingService(db);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                new CreateBookingRequest
                {
                    EventId = event1.Id,
                    SeatIds = [seat.Id]
                },
                "customer-1"));
    }

    [Fact]
    public async Task CreateAsync_WithParking_HoldsParking()
    {
        await using var db = CreateDbContext();

        var eventEntity = CreateEvent();
        db.Events.Add(eventEntity);
        await db.SaveChangesAsync();

        var seat = CreateSeat(eventEntity.Id, 1, "A1");

        var parking = new ParkingSlot
        {
            EventId = eventEntity.Id,
            Zone = "A",
            SlotNumber = 1,
            Status = "Available"
        };

        db.Seats.Add(seat);
        db.ParkingSlots.Add(parking);
        await db.SaveChangesAsync();

        var service = CreateBookingService(db);

        var result = await service.CreateAsync(
            new CreateBookingRequest
            {
                EventId = eventEntity.Id,
                SeatIds = [seat.Id],
                ParkingSlotId = parking.Id
            },
            "customer-1");

        Assert.Equal(parking.Id, result.ParkingSlotId);

        var savedParking =
            await db.ParkingSlots.SingleAsync();

        Assert.Equal("Held", savedParking.Status);

        Assert.Single(
            await db.ParkingReservations.ToListAsync());
    }

    [Fact]
    public async Task CancelAsync_HeldBooking_ReleasesSeatsAndParking()
    {
        await using var db = CreateDbContext();

        var eventEntity = CreateEvent();
        db.Events.Add(eventEntity);
        await db.SaveChangesAsync();

        var seat1 = CreateSeat(eventEntity.Id, 1, "A1");
        var seat2 = CreateSeat(eventEntity.Id, 2, "A2");

        var parking = new ParkingSlot
        {
            EventId = eventEntity.Id,
            Zone = "A",
            SlotNumber = 1,
            Status = "Available"
        };

        db.Seats.AddRange(seat1, seat2);
        db.ParkingSlots.Add(parking);
        await db.SaveChangesAsync();

        var service = CreateBookingService(db);

        var booking = await service.CreateAsync(
            new CreateBookingRequest
            {
                EventId = eventEntity.Id,
                SeatIds = [seat1.Id, seat2.Id],
                ParkingSlotId = parking.Id
            },
            "customer-1");

        var cancelled = await service.CancelAsync(
            booking.Id,
            "customer-1");

        Assert.Equal("Cancelled", cancelled.Status);

        var seats = await db.Seats.ToListAsync();

        Assert.All(
            seats,
            seat => Assert.Equal("Available", seat.Status));

        Assert.Equal(
            "Available",
            (await db.ParkingSlots.SingleAsync()).Status);

        var cancellationNotifications =
            await db.Notifications
                .OrderBy(notification => notification.Id)
                .ToListAsync();

        Assert.Equal(2, cancellationNotifications.Count);

        Assert.Contains(
            cancellationNotifications,
            notification =>
                notification.Message.Contains(
                    "was created and is being held for payment."));

        Assert.Contains(
            cancellationNotifications,
            notification =>
                notification.Message.Contains(
                    "was cancelled."));
    }

    [Fact]
    public async Task ExpireHeldBookingsAsync_ReleasesResources()
    {
        await using var db = CreateDbContext();

        var eventEntity = CreateEvent();
        db.Events.Add(eventEntity);
        await db.SaveChangesAsync();

        var seat = CreateSeat(eventEntity.Id, 1, "A1");
        db.Seats.Add(seat);
        await db.SaveChangesAsync();

        var service = CreateBookingService(
            db,
            holdMinutes: 15);

        var booking = await service.CreateAsync(
            new CreateBookingRequest
            {
                EventId = eventEntity.Id,
                SeatIds = [seat.Id]
            },
            "customer-1");

        var savedBooking =
            await db.Bookings.SingleAsync();

        savedBooking.CreatedAt =
            DateTime.UtcNow.AddMinutes(-20);

        await db.SaveChangesAsync();

        var expired =
            await service.ExpireHeldBookingsAsync();

        Assert.Equal(1, expired);

        Assert.Equal(
            "Expired",
            (await db.Bookings.SingleAsync()).Status);

        Assert.Equal(
            "Available",
            (await db.Seats.SingleAsync()).Status);
    }

    [Fact]
    public async Task ProcessAsync_MultipleSeats_ComputesServerSideTotal()
    {
        await using var db = CreateDbContext();

        var eventEntity = CreateEvent();
        eventEntity.TicketPrice = 1000m;
        eventEntity.ParkingFee = 500m;

        db.Events.Add(eventEntity);
        await db.SaveChangesAsync();

        var seat1 = CreateSeat(eventEntity.Id, 1, "A1");
        var seat2 = CreateSeat(eventEntity.Id, 2, "A2");

        var parking = new ParkingSlot
        {
            EventId = eventEntity.Id,
            Zone = "A",
            SlotNumber = 1,
            Status = "Available"
        };

        db.Seats.AddRange(seat1, seat2);
        db.ParkingSlots.Add(parking);
        await db.SaveChangesAsync();

        var bookingService = CreateBookingService(db);

        var booking = await bookingService.CreateAsync(
            new CreateBookingRequest
            {
                EventId = eventEntity.Id,
                SeatIds = [seat1.Id, seat2.Id],
                ParkingSlotId = parking.Id
            },
            "customer-1");

        var paymentService =
            new PaymentService(
                db,
                Options.Create(
                    new BookingSettings
                    {
                        HoldMinutes = 15
                    }));

        var payment = await paymentService.ProcessAsync(
            new ProcessPaymentRequest
            {
                BookingId = booking.Id,
                PaymentMethod = "Card"
            },
            "customer-1");

        Assert.Equal(2500m, payment.Amount);
        Assert.Equal("Completed", payment.Status);

        var savedBooking =
            await db.Bookings.SingleAsync();

        Assert.Equal("Confirmed", savedBooking.Status);
        Assert.Equal("Completed", savedBooking.PaymentStatus);

        var seats = await db.Seats.ToListAsync();

        Assert.All(
            seats,
            seat => Assert.Equal("Booked", seat.Status));

        Assert.Equal(
            "Occupied",
            (await db.ParkingSlots.SingleAsync()).Status);

        var paymentNotifications =
            await db.Notifications
                .OrderBy(notification => notification.Id)
                .ToListAsync();

        Assert.Equal(2, paymentNotifications.Count);

        Assert.Contains(
            paymentNotifications,
            notification =>
                notification.Message.Contains(
                    "was created and is being held for payment."));

        Assert.Contains(
            paymentNotifications,
            notification =>
                notification.Message.Contains(
                    "Payment completed for booking"));
    }

    [Fact]
    public async Task ProcessAsync_ExpiredHold_RejectsAndReleasesSeat()
    {
        await using var db = CreateDbContext();

        var eventEntity = CreateEvent();
        db.Events.Add(eventEntity);
        await db.SaveChangesAsync();

        var seat = CreateSeat(eventEntity.Id, 1, "A1");
        db.Seats.Add(seat);
        await db.SaveChangesAsync();

        var bookingService =
            CreateBookingService(db);

        var booking = await bookingService.CreateAsync(
            new CreateBookingRequest
            {
                EventId = eventEntity.Id,
                SeatIds = [seat.Id]
            },
            "customer-1");

        var savedBooking =
            await db.Bookings.SingleAsync();

        savedBooking.CreatedAt =
            DateTime.UtcNow.AddMinutes(-20);

        await db.SaveChangesAsync();

        var paymentService =
            new PaymentService(
                db,
                Options.Create(
                    new BookingSettings
                    {
                        HoldMinutes = 15
                    }));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => paymentService.ProcessAsync(
                new ProcessPaymentRequest
                {
                    BookingId = booking.Id,
                    PaymentMethod = "Card"
                },
                "customer-1"));

        Assert.Equal(
            "Expired",
            (await db.Bookings.SingleAsync()).Status);

        Assert.Equal(
            "Available",
            (await db.Seats.SingleAsync()).Status);

        Assert.Empty(
            await db.Payments.ToListAsync());
    }

    [Fact]
    public async Task ProcessAsync_AfterSuccessfulPayment_RejectsSecondPayment()
    {
        await using var db = CreateDbContext();

        var eventEntity = CreateEvent();
        db.Events.Add(eventEntity);
        await db.SaveChangesAsync();

        var seat = CreateSeat(eventEntity.Id, 1, "A1");
        db.Seats.Add(seat);
        await db.SaveChangesAsync();

        var bookingService = CreateBookingService(db);

        var booking = await bookingService.CreateAsync(
            new CreateBookingRequest
            {
                EventId = eventEntity.Id,
                SeatIds = [seat.Id]
            },
            "customer-1");

        var paymentService = new PaymentService(db);

        var request = new ProcessPaymentRequest
        {
            BookingId = booking.Id,
            PaymentMethod = "Card"
        };

        await paymentService.ProcessAsync(
            request,
            "customer-1");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => paymentService.ProcessAsync(
                request,
                "customer-1"));

        Assert.Single(
            await db.Payments.ToListAsync());
    }

    private static BookingService CreateBookingService(
        AppDbContext db,
        int holdMinutes = 15)
    {
        return new BookingService(
            db,
            Options.Create(
                new BookingSettings
                {
                    HoldMinutes = holdMinutes,
                    ExpiryScanSeconds = 60
                }));
    }

    private static Event CreateEvent(
        string name = "Test Event")
    {
        return new Event
        {
            Name = name,
            Capacity = 100,
            TicketPrice = 1000m,
            ParkingFee = 500m,
            StartDateTimeUtc =
                DateTime.UtcNow.AddDays(1),
            EndDateTimeUtc =
                DateTime.UtcNow.AddDays(1).AddHours(3)
        };
    }

    private static Seat CreateSeat(
        int eventId,
        int number,
        string label)
    {
        return new Seat
        {
            EventId = eventId,
            RowLabel = "A",
            SeatNumber = number,
            DisplayLabel = label,
            Status = "Available"
        };
    }

    private static AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        return new AppDbContext(options);
    }
}
