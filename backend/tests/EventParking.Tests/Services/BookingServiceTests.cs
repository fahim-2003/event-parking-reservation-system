using EventParking.API.Data;
using EventParking.API.DTOs.Bookings;
using EventParking.API.Entities;
using EventParking.API.Services;
using Microsoft.EntityFrameworkCore;

namespace EventParking.Tests.Services;

public sealed class BookingServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenSeatAvailable_CreatesBooking()
    {
        await using var dbContext = CreateDbContext();

        var eventEntity = new Event
        {
            Name = "Test Event",
            Capacity = 1
        };

        dbContext.Events.Add(eventEntity);

        var seat = new Seat
        {
            EventId = eventEntity.Id,
            RowLabel = "A",
            SeatNumber = 1,
            DisplayLabel = "A1",
            Status = "Available"
        };

        dbContext.Seats.Add(seat);
        await dbContext.SaveChangesAsync();

        var service = new BookingService(dbContext);

        var result = await service.CreateAsync(
            new CreateBookingRequest
            {
                EventId = eventEntity.Id,
                SeatId = seat.Id
            },
            "customer-1");

        Assert.Equal("Held", result.Status);

        Assert.Equal(
            "Pending",
            result.PaymentStatus);

        Assert.NotEmpty(
            result.BookingNumber);

        var savedSeat = await dbContext.Seats
            .FirstAsync();

        Assert.Equal(
            "Booked",
            savedSeat.Status);

        Assert.Single(
            await dbContext.Bookings.ToListAsync());
    }

    [Fact]
    public async Task CreateAsync_WhenSeatAlreadyBooked_ThrowsConflict()
    {
        await using var dbContext = CreateDbContext();

        var eventEntity = new Event
        {
            Name = "Test Event",
            Capacity = 1
        };

        dbContext.Events.Add(eventEntity);

        var seat = new Seat
        {
            EventId = eventEntity.Id,
            RowLabel = "A",
            SeatNumber = 1,
            DisplayLabel = "A1",
            Status = "Booked"
        };

        dbContext.Seats.Add(seat);

        await dbContext.SaveChangesAsync();

        var service = new BookingService(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(
                new CreateBookingRequest
                {
                    EventId = eventEntity.Id,
                    SeatId = seat.Id
                },
                "customer-1"));
    }


    [Fact]
    public async Task CreateAsync_WithoutSeat_ThrowsValidationError()
    {
        await using var dbContext = CreateDbContext();

        var eventEntity = new Event
        {
            Name = "Test Event",
            Capacity = 1
        };

        dbContext.Events.Add(eventEntity);

        await dbContext.SaveChangesAsync();

        var service = new BookingService(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(
                new CreateBookingRequest
                {
                    EventId = eventEntity.Id
                },
                "customer-1"));
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



