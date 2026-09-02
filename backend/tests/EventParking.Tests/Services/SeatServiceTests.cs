using EventParking.API.Data;
using EventParking.API.DTOs.Seats;
using EventParking.API.Entities;
using EventParking.API.Services;
using Microsoft.EntityFrameworkCore;
using EventEntity = EventParking.API.Entities.Event;

namespace EventParking.Tests.Services;

public sealed class SeatServiceTests
{
    [Fact]
    public async Task GenerateAsync_WhenSeatCountMatchesCapacity_CreatesSeatMap()
    {
        await using var dbContext = CreateDbContext();
        var eventEntity = await SeedEventAsync(dbContext, capacity: 6);
        var service = new SeatService(dbContext);

        var request = new GenerateSeatMapRequest
        {
            Rows = 2,
            SeatsPerRow = 3
        };

        var result = await service.GenerateAsync(
            eventEntity.Id,
            request);

        Assert.Equal(6, result.Count);

        Assert.Equal(
            new[] { "A1", "A2", "A3", "B1", "B2", "B3" },
            result.Select(seat => seat.DisplayLabel).ToArray());

        Assert.All(
            result,
            seat => Assert.Equal("Available", seat.Status));

        Assert.Equal(
            6,
            await dbContext.Seats.CountAsync());
    }

    [Fact]
    public async Task GenerateAsync_WhenSeatCountDoesNotMatchCapacity_ThrowsArgumentException()
    {
        await using var dbContext = CreateDbContext();
        var eventEntity = await SeedEventAsync(dbContext, capacity: 6);
        var service = new SeatService(dbContext);

        var request = new GenerateSeatMapRequest
        {
            Rows = 2,
            SeatsPerRow = 2
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.GenerateAsync(
                eventEntity.Id,
                request));

        Assert.Contains(
            "exactly 6 seats",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateAsync_WhenLayoutAlreadyExists_ThrowsInvalidOperationException()
    {
        await using var dbContext = CreateDbContext();
        var eventEntity = await SeedEventAsync(dbContext, capacity: 4);
        var service = new SeatService(dbContext);

        var request = new GenerateSeatMapRequest
        {
            Rows = 2,
            SeatsPerRow = 2
        };

        await service.GenerateAsync(
            eventEntity.Id,
            request);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.GenerateAsync(
                    eventEntity.Id,
                    request));

        Assert.Contains(
            "already exists",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetByEventAsync_ReturnsOnlySeatsForRequestedEvent()
    {
        await using var dbContext = CreateDbContext();

        var firstEvent = await SeedEventAsync(
            dbContext,
            capacity: 2,
            name: "First Event");

        var secondEvent = await SeedEventAsync(
            dbContext,
            capacity: 1,
            name: "Second Event");

        dbContext.Seats.AddRange(
            new Seat
            {
                EventId = firstEvent.Id,
                RowLabel = "A",
                SeatNumber = 2,
                DisplayLabel = "A2",
                Status = "Available"
            },
            new Seat
            {
                EventId = firstEvent.Id,
                RowLabel = "A",
                SeatNumber = 1,
                DisplayLabel = "A1",
                Status = "Available"
            },
            new Seat
            {
                EventId = secondEvent.Id,
                RowLabel = "A",
                SeatNumber = 1,
                DisplayLabel = "A1",
                Status = "Available"
            });

        await dbContext.SaveChangesAsync();

        var service = new SeatService(dbContext);

        var result = await service.GetByEventAsync(
            firstEvent.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal("A1", result[0].DisplayLabel);
        Assert.Equal("A2", result[1].DisplayLabel);

        Assert.All(
            result,
            seat => Assert.Equal(firstEvent.Id, seat.EventId));
    }

    [Fact]
    public async Task UpdateAsync_WhenSeatIsHeld_ThrowsInvalidOperationException()
    {
        await using var dbContext = CreateDbContext();
        var eventEntity = await SeedEventAsync(dbContext, capacity: 1);

        var seat = new Seat
        {
            EventId = eventEntity.Id,
            RowLabel = "A",
            SeatNumber = 1,
            DisplayLabel = "A1",
            Status = "Held",
            RowVersion = [1]
        };

        dbContext.Seats.Add(seat);
        await dbContext.SaveChangesAsync();

        var service = new SeatService(dbContext);

        var request = new UpdateSeatRequest
        {
            RowLabel = "B",
            SeatNumber = 1,
            RowVersion = [1]
        };

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAsync(
                    eventEntity.Id,
                    seat.Id,
                    request));

        Assert.Contains(
            "cannot be renamed",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteAsync_WhenSeatIsBooked_ThrowsInvalidOperationException()
    {
        await using var dbContext = CreateDbContext();
        var eventEntity = await SeedEventAsync(dbContext, capacity: 1);

        var seat = new Seat
        {
            EventId = eventEntity.Id,
            RowLabel = "A",
            SeatNumber = 1,
            DisplayLabel = "A1",
            Status = "Booked",
            RowVersion = [1]
        };

        dbContext.Seats.Add(seat);
        await dbContext.SaveChangesAsync();

        var service = new SeatService(dbContext);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteAsync(
                    eventEntity.Id,
                    seat.Id,
                    [1]));

        Assert.Contains(
            "cannot be deleted",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_WhenSeatBelongsToAnotherEvent_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();

        var firstEvent = await SeedEventAsync(
            dbContext,
            capacity: 1,
            name: "First Event");

        var secondEvent = await SeedEventAsync(
            dbContext,
            capacity: 1,
            name: "Second Event");

        var seat = new Seat
        {
            EventId = firstEvent.Id,
            RowLabel = "A",
            SeatNumber = 1,
            DisplayLabel = "A1",
            Status = "Available",
            RowVersion = [1]
        };

        dbContext.Seats.Add(seat);
        await dbContext.SaveChangesAsync();

        var service = new SeatService(dbContext);

        var request = new UpdateSeatRequest
        {
            RowLabel = "B",
            SeatNumber = 1,
            RowVersion = [1]
        };

        var result = await service.UpdateAsync(
            secondEvent.Id,
            seat.Id,
            request);

        Assert.Null(result);
    }

    private static async Task<EventEntity> SeedEventAsync(
        AppDbContext dbContext,
        int capacity,
        string name = "Sample Event")
    {
        var timestamp = DateTime.UtcNow;

        var venue = new Venue
        {
            Name = $"{name} Venue",
            Address = "10 Test Road",
            Capacity = Math.Max(capacity, 100),
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };

        var category = new EventCategory
        {
            Name = $"{name} Category",
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };

        dbContext.Venues.Add(venue);
        dbContext.EventCategories.Add(category);

        await dbContext.SaveChangesAsync();

        var eventEntity = new EventEntity
        {
            Name = name,
            VenueId = venue.Id,
            EventCategoryId = category.Id,
            StartDateTimeUtc = new DateTime(
                2027, 1, 1, 9, 0, 0, DateTimeKind.Utc),
            EndDateTimeUtc = new DateTime(
                2027, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            TicketPrice = 500m,
            ParkingFee = 100m,
            Capacity = capacity,
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };

        dbContext.Events.Add(eventEntity);
        await dbContext.SaveChangesAsync();

        return eventEntity;
    }

    private static AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(
                    $"SeatServiceTests-{Guid.NewGuid()}")
                .Options;

        return new AppDbContext(options);
    }
}
