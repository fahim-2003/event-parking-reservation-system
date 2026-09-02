using EventParking.API.Data;
using EventParking.API.DTOs.Events;
using EventParking.API.Entities;
using EventParking.API.Services;
using Microsoft.EntityFrameworkCore;
using EventEntity = EventParking.API.Entities.Event;

namespace EventParking.Tests.Services;

public sealed class EventServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesEvent()
    {
        await using var dbContext = CreateDbContext();
        var (venue, category) = await SeedVenueAndCategoryAsync(dbContext);
        var service = new EventService(dbContext);

        var request = new CreateEventRequest
        {
            Name = "  Tech Conference  ",
            Description = "  Annual technology event  ",
            VenueId = venue.Id,
            EventCategoryId = category.Id,
            StartDateTimeUtc = new DateTime(
                2026, 10, 10, 9, 0, 0, DateTimeKind.Utc),
            EndDateTimeUtc = new DateTime(
                2026, 10, 10, 17, 0, 0, DateTimeKind.Utc),
            TicketPrice = 1500m,
            ParkingFee = 200m,
            Capacity = 400
        };

        var result = await service.CreateAsync(request);

        Assert.True(result.Id > 0);
        Assert.Equal("Tech Conference", result.Name);
        Assert.Equal("Annual technology event", result.Description);
        Assert.Equal(venue.Id, result.VenueId);
        Assert.Equal("Main Hall", result.VenueName);
        Assert.Equal(category.Id, result.EventCategoryId);
        Assert.Equal("Conference", result.CategoryName);
        Assert.Equal(400, result.Capacity);
        Assert.Equal(1500m, result.TicketPrice);
        Assert.Equal(200m, result.ParkingFee);

        var persistedEvent = await dbContext.Events.SingleAsync();

        Assert.Equal("Tech Conference", persistedEvent.Name);
    }

    [Fact]
    public async Task CreateAsync_WhenCapacityExceedsVenue_ThrowsArgumentException()
    {
        await using var dbContext = CreateDbContext();
        var (venue, category) = await SeedVenueAndCategoryAsync(dbContext);
        var service = new EventService(dbContext);

        var request = CreateValidRequest(
            venue.Id,
            category.Id);

        request.Capacity = venue.Capacity + 1;

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(request));

        Assert.Contains(
            "cannot exceed",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WhenVenueOverlaps_ThrowsInvalidOperationException()
    {
        await using var dbContext = CreateDbContext();
        var (venue, category) = await SeedVenueAndCategoryAsync(dbContext);

        dbContext.Events.Add(new EventEntity
        {
            Name = "Existing Event",
            VenueId = venue.Id,
            EventCategoryId = category.Id,
            StartDateTimeUtc = new DateTime(
                2026, 10, 10, 10, 0, 0, DateTimeKind.Utc),
            EndDateTimeUtc = new DateTime(
                2026, 10, 10, 14, 0, 0, DateTimeKind.Utc),
            TicketPrice = 100m,
            ParkingFee = 50m,
            Capacity = 100,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var service = new EventService(dbContext);

        var request = CreateValidRequest(
            venue.Id,
            category.Id);

        request.StartDateTimeUtc = new DateTime(
            2026, 10, 10, 12, 0, 0, DateTimeKind.Utc);

        request.EndDateTimeUtc = new DateTime(
            2026, 10, 10, 16, 0, 0, DateTimeKind.Utc);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(request));

        Assert.Contains(
            "overlapping",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryDoesNotExist_ThrowsArgumentException()
    {
        await using var dbContext = CreateDbContext();
        var (venue, _) = await SeedVenueAndCategoryAsync(dbContext);
        var service = new EventService(dbContext);

        var request = CreateValidRequest(
            venue.Id,
            999);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(request));

        Assert.Contains(
            "category",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_SameEventTime_DoesNotConflictWithItself()
    {
        await using var dbContext = CreateDbContext();
        var (venue, category) = await SeedVenueAndCategoryAsync(dbContext);

        var eventEntity = new EventEntity
        {
            Name = "Original Event",
            VenueId = venue.Id,
            EventCategoryId = category.Id,
            StartDateTimeUtc = new DateTime(
                2026, 11, 1, 9, 0, 0, DateTimeKind.Utc),
            EndDateTimeUtc = new DateTime(
                2026, 11, 1, 12, 0, 0, DateTimeKind.Utc),
            TicketPrice = 500m,
            ParkingFee = 100m,
            Capacity = 200,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.Events.Add(eventEntity);
        await dbContext.SaveChangesAsync();

        var service = new EventService(dbContext);

        var request = new UpdateEventRequest
        {
            Name = "Updated Event",
            Description = "Updated description",
            VenueId = venue.Id,
            EventCategoryId = category.Id,
            StartDateTimeUtc = eventEntity.StartDateTimeUtc,
            EndDateTimeUtc = eventEntity.EndDateTimeUtc,
            TicketPrice = 600m,
            ParkingFee = 120m,
            Capacity = 220
        };

        var result = await service.UpdateAsync(
            eventEntity.Id,
            request);

        Assert.NotNull(result);
        Assert.Equal("Updated Event", result.Name);
        Assert.Equal(220, result.Capacity);
    }

    [Fact]
    public async Task GetAllAsync_AppliesSearchVenueAndCategoryFilters()
    {
        await using var dbContext = CreateDbContext();
        var (venue, category) = await SeedVenueAndCategoryAsync(dbContext);

        var secondVenue = new Venue
        {
            Name = "Outdoor Arena",
            Address = "Arena Road",
            Capacity = 1000,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.Venues.Add(secondVenue);
        await dbContext.SaveChangesAsync();

        dbContext.Events.AddRange(
            new EventEntity
            {
                Name = "Tech Expo",
                Description = "Technology exhibition",
                VenueId = venue.Id,
                EventCategoryId = category.Id,
                StartDateTimeUtc = new DateTime(
                    2026, 12, 1, 9, 0, 0, DateTimeKind.Utc),
                EndDateTimeUtc = new DateTime(
                    2026, 12, 1, 12, 0, 0, DateTimeKind.Utc),
                TicketPrice = 1000m,
                ParkingFee = 100m,
                Capacity = 300,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            },
            new EventEntity
            {
                Name = "Music Night",
                Description = "Live music",
                VenueId = secondVenue.Id,
                EventCategoryId = category.Id,
                StartDateTimeUtc = new DateTime(
                    2026, 12, 2, 18, 0, 0, DateTimeKind.Utc),
                EndDateTimeUtc = new DateTime(
                    2026, 12, 2, 22, 0, 0, DateTimeKind.Utc),
                TicketPrice = 2000m,
                ParkingFee = 150m,
                Capacity = 800,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync();

        var service = new EventService(dbContext);

        var result = await service.GetAllAsync(
            search: "Tech",
            venueId: venue.Id,
            categoryId: category.Id);

        var eventResponse = Assert.Single(result);

        Assert.Equal("Tech Expo", eventResponse.Name);
        Assert.Equal(venue.Id, eventResponse.VenueId);
        Assert.Equal(category.Id, eventResponse.EventCategoryId);
    }

    [Fact]
    public async Task DeleteAsync_WhenEventExists_RemovesEvent()
    {
        await using var dbContext = CreateDbContext();
        var (venue, category) = await SeedVenueAndCategoryAsync(dbContext);

        var eventEntity = new EventEntity
        {
            Name = "Temporary Event",
            VenueId = venue.Id,
            EventCategoryId = category.Id,
            StartDateTimeUtc = new DateTime(
                2027, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            EndDateTimeUtc = new DateTime(
                2027, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            TicketPrice = 0m,
            ParkingFee = 0m,
            Capacity = 100,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.Events.Add(eventEntity);
        await dbContext.SaveChangesAsync();

        var service = new EventService(dbContext);

        var deleted = await service.DeleteAsync(eventEntity.Id);

        Assert.True(deleted);
        Assert.Empty(await dbContext.Events.ToListAsync());
    }

    private static CreateEventRequest CreateValidRequest(
        int venueId,
        int categoryId)
    {
        return new CreateEventRequest
        {
            Name = "Sample Event",
            VenueId = venueId,
            EventCategoryId = categoryId,
            StartDateTimeUtc = new DateTime(
                2026, 10, 10, 9, 0, 0, DateTimeKind.Utc),
            EndDateTimeUtc = new DateTime(
                2026, 10, 10, 12, 0, 0, DateTimeKind.Utc),
            TicketPrice = 500m,
            ParkingFee = 100m,
            Capacity = 200
        };
    }

    private static async Task<(Venue Venue, EventCategory Category)>
        SeedVenueAndCategoryAsync(AppDbContext dbContext)
    {
        var timestamp = DateTime.UtcNow;

        var venue = new Venue
        {
            Name = "Main Hall",
            Address = "10 Central Road",
            Capacity = 500,
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };

        var category = new EventCategory
        {
            Name = "Conference",
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };

        dbContext.Venues.Add(venue);
        dbContext.EventCategories.Add(category);

        await dbContext.SaveChangesAsync();

        return (venue, category);
    }

    private static AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(
                    $"EventServiceTests-{Guid.NewGuid()}")
                .Options;

        return new AppDbContext(options);
    }
}
