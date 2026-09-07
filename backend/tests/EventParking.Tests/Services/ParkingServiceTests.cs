using EventParking.API.Data;
using EventParking.API.DTOs.Parking;
using EventParking.API.Entities;
using EventParking.API.Services;
using Microsoft.EntityFrameworkCore;
using EventEntity = EventParking.API.Entities.Event;

namespace EventParking.Tests.Services;

public sealed class ParkingServiceTests
{
    [Fact]
    public async Task GenerateAsync_ValidRequest_CreatesParkingLayout()
    {
        await using var dbContext = CreateDbContext();
        var eventEntity = await SeedEventAsync(dbContext);
        var service = new ParkingService(dbContext);

        var request = new GenerateParkingLayoutRequest
        {
            Zone = " zone a ",
            NumberOfSlots = 3
        };

        var result = await service.GenerateAsync(
            eventEntity.Id,
            request);

        Assert.Equal(3, result.Count);

        Assert.All(
            result,
            slot =>
            {
                Assert.Equal("ZONE A", slot.Zone);
                Assert.Equal("Available", slot.Status);
            });

        Assert.Equal(
            new[] { 1, 2, 3 },
            result.Select(slot => slot.SlotNumber).ToArray());

        Assert.Equal(
            3,
            await dbContext.ParkingSlots.CountAsync());
    }

    [Fact]
    public async Task GenerateAsync_WhenEventDoesNotExist_ThrowsArgumentException()
    {
        await using var dbContext = CreateDbContext();
        var service = new ParkingService(dbContext);

        var request = new GenerateParkingLayoutRequest
        {
            Zone = "A",
            NumberOfSlots = 3
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.GenerateAsync(
                999,
                request));

        Assert.Contains(
            "event does not exist",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateAsync_WhenZoneAlreadyExists_ThrowsInvalidOperationException()
    {
        await using var dbContext = CreateDbContext();
        var eventEntity = await SeedEventAsync(dbContext);
        var service = new ParkingService(dbContext);

        var request = new GenerateParkingLayoutRequest
        {
            Zone = "Zone A",
            NumberOfSlots = 2
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
    public async Task GetByEventAsync_ReturnsOnlyRequestedEventSlots()
    {
        await using var dbContext = CreateDbContext();

        var firstEvent = await SeedEventAsync(
            dbContext,
            "First Event");

        var secondEvent = await SeedEventAsync(
            dbContext,
            "Second Event");

        dbContext.ParkingSlots.AddRange(
            new ParkingSlot
            {
                EventId = firstEvent.Id,
                Zone = "A",
                SlotNumber = 2,
                Status = "Available"
            },
            new ParkingSlot
            {
                EventId = firstEvent.Id,
                Zone = "A",
                SlotNumber = 1,
                Status = "Available"
            },
            new ParkingSlot
            {
                EventId = secondEvent.Id,
                Zone = "B",
                SlotNumber = 1,
                Status = "Available"
            });

        await dbContext.SaveChangesAsync();

        var service = new ParkingService(dbContext);

        var result = await service.GetByEventAsync(
            firstEvent.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].SlotNumber);
        Assert.Equal(2, result[1].SlotNumber);

        Assert.All(
            result,
            slot => Assert.Equal(firstEvent.Id, slot.EventId));
    }

    [Fact]
    public async Task UpdateAsync_WhenSlotIsHeld_ThrowsInvalidOperationException()
    {
        await using var dbContext = CreateDbContext();
        var eventEntity = await SeedEventAsync(dbContext);

        var parkingSlot = new ParkingSlot
        {
            EventId = eventEntity.Id,
            Zone = "A",
            SlotNumber = 1,
            Status = "Held",
            RowVersion = [1]
        };

        dbContext.ParkingSlots.Add(parkingSlot);
        await dbContext.SaveChangesAsync();

        var service = new ParkingService(dbContext);

        var request = new UpdateParkingSlotRequest
        {
            Zone = "B",
            SlotNumber = 1,
            RowVersion = [1]
        };

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAsync(
                    eventEntity.Id,
                    parkingSlot.Id,
                    request));

        Assert.Contains(
            "cannot be renamed",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteAsync_WhenSlotIsOccupied_ThrowsInvalidOperationException()
    {
        await using var dbContext = CreateDbContext();
        var eventEntity = await SeedEventAsync(dbContext);

        var parkingSlot = new ParkingSlot
        {
            EventId = eventEntity.Id,
            Zone = "A",
            SlotNumber = 1,
            Status = "Occupied",
            RowVersion = [1]
        };

        dbContext.ParkingSlots.Add(parkingSlot);
        await dbContext.SaveChangesAsync();

        var service = new ParkingService(dbContext);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteAsync(
                    eventEntity.Id,
                    parkingSlot.Id,
                    [1]));

        Assert.Contains(
            "cannot be deleted",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_WhenSlotBelongsToAnotherEvent_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();

        var firstEvent = await SeedEventAsync(
            dbContext,
            "First Event");

        var secondEvent = await SeedEventAsync(
            dbContext,
            "Second Event");

        var parkingSlot = new ParkingSlot
        {
            EventId = firstEvent.Id,
            Zone = "A",
            SlotNumber = 1,
            Status = "Available",
            RowVersion = [1]
        };

        dbContext.ParkingSlots.Add(parkingSlot);
        await dbContext.SaveChangesAsync();

        var service = new ParkingService(dbContext);

        var request = new UpdateParkingSlotRequest
        {
            Zone = "B",
            SlotNumber = 1,
            RowVersion = [1]
        };

        var result = await service.UpdateAsync(
            secondEvent.Id,
            parkingSlot.Id,
            request);

        Assert.Null(result);
    }

    private static async Task<EventEntity> SeedEventAsync(
        AppDbContext dbContext,
        string name = "Parking Event")
    {
        var timestamp = DateTime.UtcNow;

        var venue = new Venue
        {
            Name = $"{name} Venue",
            Address = "20 Parking Road",
            Capacity = 500,
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
                2027, 2, 1, 9, 0, 0, DateTimeKind.Utc),
            EndDateTimeUtc = new DateTime(
                2027, 2, 1, 12, 0, 0, DateTimeKind.Utc),
            TicketPrice = 500m,
            ParkingFee = 100m,
            Capacity = 200,
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
                    $"ParkingServiceTests-{Guid.NewGuid()}")
                .Options;

        return new AppDbContext(options);
    }
}
