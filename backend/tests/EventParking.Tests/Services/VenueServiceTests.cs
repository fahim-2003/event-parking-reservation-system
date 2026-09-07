using EventParking.API.Data;
using EventParking.API.DTOs.Venues;
using EventParking.API.Entities;
using EventParking.API.Services;
using Microsoft.EntityFrameworkCore;

namespace EventParking.Tests.Services;

public sealed class VenueServiceTests
{
    [Fact]
    public async Task CreateAsync_TrimsValuesAndPersistsVenue()
    {
        await using var dbContext = CreateDbContext();
        var service = new VenueService(dbContext);

        var request = new CreateVenueRequest
        {
            Name = "  Main Hall  ",
            Address = "  10 Central Road  ",
            Capacity = 500
        };

        var beforeCreate = DateTime.UtcNow;

        var result = await service.CreateAsync(request);

        var afterCreate = DateTime.UtcNow;

        Assert.True(result.Id > 0);
        Assert.Equal("Main Hall", result.Name);
        Assert.Equal("10 Central Road", result.Address);
        Assert.Equal(500, result.Capacity);
        Assert.InRange(result.CreatedAtUtc, beforeCreate, afterCreate);
        Assert.Equal(result.CreatedAtUtc, result.UpdatedAtUtc);

        var persistedVenue = await dbContext.Venues.SingleAsync();

        Assert.Equal("Main Hall", persistedVenue.Name);
        Assert.Equal("10 Central Road", persistedVenue.Address);
        Assert.Equal(500, persistedVenue.Capacity);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsVenuesOrderedByName()
    {
        await using var dbContext = CreateDbContext();

        var timestamp = DateTime.UtcNow;

        dbContext.Venues.AddRange(
            new Venue
            {
                Name = "West Hall",
                Address = "West Road",
                Capacity = 200,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            },
            new Venue
            {
                Name = "Main Hall",
                Address = "Central Road",
                Capacity = 500,
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            });

        await dbContext.SaveChangesAsync();

        var service = new VenueService(dbContext);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Main Hall", result[0].Name);
        Assert.Equal("West Hall", result[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenVenueExists_ReturnsVenue()
    {
        await using var dbContext = CreateDbContext();

        var timestamp = DateTime.UtcNow;

        var venue = new Venue
        {
            Name = "Conference Hall",
            Address = "Lake Road",
            Capacity = 300,
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };

        dbContext.Venues.Add(venue);
        await dbContext.SaveChangesAsync();

        var service = new VenueService(dbContext);

        var result = await service.GetByIdAsync(venue.Id);

        Assert.NotNull(result);
        Assert.Equal(venue.Id, result.Id);
        Assert.Equal("Conference Hall", result.Name);
        Assert.Equal("Lake Road", result.Address);
        Assert.Equal(300, result.Capacity);
    }

    [Fact]
    public async Task UpdateAsync_WhenVenueExists_UpdatesVenue()
    {
        await using var dbContext = CreateDbContext();

        var originalTimestamp = new DateTime(
            2026,
            1,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var venue = new Venue
        {
            Name = "Old Hall",
            Address = "Old Road",
            Capacity = 100,
            CreatedAtUtc = originalTimestamp,
            UpdatedAtUtc = originalTimestamp
        };

        dbContext.Venues.Add(venue);
        await dbContext.SaveChangesAsync();

        var service = new VenueService(dbContext);

        var request = new UpdateVenueRequest
        {
            Name = "  Updated Hall  ",
            Address = "  New Road  ",
            Capacity = 250
        };

        var beforeUpdate = DateTime.UtcNow;

        var result = await service.UpdateAsync(venue.Id, request);

        var afterUpdate = DateTime.UtcNow;

        Assert.NotNull(result);
        Assert.Equal("Updated Hall", result.Name);
        Assert.Equal("New Road", result.Address);
        Assert.Equal(250, result.Capacity);
        Assert.Equal(originalTimestamp, result.CreatedAtUtc);
        Assert.InRange(result.UpdatedAtUtc, beforeUpdate, afterUpdate);
    }

    [Fact]
    public async Task UpdateAsync_WhenVenueDoesNotExist_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var service = new VenueService(dbContext);

        var request = new UpdateVenueRequest
        {
            Name = "Unknown Hall",
            Address = "Unknown Road",
            Capacity = 100
        };

        var result = await service.UpdateAsync(999, request);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenVenueExists_RemovesVenue()
    {
        await using var dbContext = CreateDbContext();

        var timestamp = DateTime.UtcNow;

        var venue = new Venue
        {
            Name = "Temporary Hall",
            Address = "Temporary Road",
            Capacity = 50,
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };

        dbContext.Venues.Add(venue);
        await dbContext.SaveChangesAsync();

        var service = new VenueService(dbContext);

        var deleted = await service.DeleteAsync(venue.Id);

        Assert.True(deleted);
        Assert.Empty(await dbContext.Venues.ToListAsync());
    }

    [Fact]
    public async Task DeleteAsync_WhenVenueDoesNotExist_ReturnsFalse()
    {
        await using var dbContext = CreateDbContext();
        var service = new VenueService(dbContext);

        var deleted = await service.DeleteAsync(999);

        Assert.False(deleted);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"VenueServiceTests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }
}
