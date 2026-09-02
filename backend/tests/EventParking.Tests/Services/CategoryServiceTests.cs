using EventParking.API.Data;
using EventParking.API.DTOs.Categories;
using EventParking.API.Entities;
using EventParking.API.Services;
using Microsoft.EntityFrameworkCore;

namespace EventParking.Tests.Services;

public sealed class CategoryServiceTests
{
    [Fact]
    public async Task CreateAsync_TrimsNameAndPersistsCategory()
    {
        await using var dbContext = CreateDbContext();
        var service = new CategoryService(dbContext);

        var request = new CreateCategoryRequest
        {
            Name = "  Concert  "
        };

        var beforeCreate = DateTime.UtcNow;

        var result = await service.CreateAsync(request);

        var afterCreate = DateTime.UtcNow;

        Assert.True(result.Id > 0);
        Assert.Equal("Concert", result.Name);
        Assert.InRange(result.CreatedAtUtc, beforeCreate, afterCreate);
        Assert.Equal(result.CreatedAtUtc, result.UpdatedAtUtc);

        var persistedCategory =
            await dbContext.EventCategories.SingleAsync();

        Assert.Equal("Concert", persistedCategory.Name);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCategoriesOrderedByName()
    {
        await using var dbContext = CreateDbContext();

        var timestamp = DateTime.UtcNow;

        dbContext.EventCategories.AddRange(
            new EventCategory
            {
                Name = "Sports",
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            },
            new EventCategory
            {
                Name = "Concert",
                CreatedAtUtc = timestamp,
                UpdatedAtUtc = timestamp
            });

        await dbContext.SaveChangesAsync();

        var service = new CategoryService(dbContext);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Concert", result[0].Name);
        Assert.Equal("Sports", result[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryExists_ReturnsCategory()
    {
        await using var dbContext = CreateDbContext();

        var timestamp = DateTime.UtcNow;

        var category = new EventCategory
        {
            Name = "Conference",
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };

        dbContext.EventCategories.Add(category);
        await dbContext.SaveChangesAsync();

        var service = new CategoryService(dbContext);

        var result = await service.GetByIdAsync(category.Id);

        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal("Conference", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryExists_UpdatesCategory()
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

        var category = new EventCategory
        {
            Name = "Old Category",
            CreatedAtUtc = originalTimestamp,
            UpdatedAtUtc = originalTimestamp
        };

        dbContext.EventCategories.Add(category);
        await dbContext.SaveChangesAsync();

        var service = new CategoryService(dbContext);

        var request = new UpdateCategoryRequest
        {
            Name = "  Updated Category  "
        };

        var beforeUpdate = DateTime.UtcNow;

        var result = await service.UpdateAsync(category.Id, request);

        var afterUpdate = DateTime.UtcNow;

        Assert.NotNull(result);
        Assert.Equal("Updated Category", result.Name);
        Assert.Equal(originalTimestamp, result.CreatedAtUtc);
        Assert.InRange(result.UpdatedAtUtc, beforeUpdate, afterUpdate);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryDoesNotExist_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var service = new CategoryService(dbContext);

        var request = new UpdateCategoryRequest
        {
            Name = "Unknown Category"
        };

        var result = await service.UpdateAsync(999, request);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryExists_RemovesCategory()
    {
        await using var dbContext = CreateDbContext();

        var timestamp = DateTime.UtcNow;

        var category = new EventCategory
        {
            Name = "Temporary Category",
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };

        dbContext.EventCategories.Add(category);
        await dbContext.SaveChangesAsync();

        var service = new CategoryService(dbContext);

        var deleted = await service.DeleteAsync(category.Id);

        Assert.True(deleted);
        Assert.Empty(
            await dbContext.EventCategories.ToListAsync());
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryDoesNotExist_ReturnsFalse()
    {
        await using var dbContext = CreateDbContext();
        var service = new CategoryService(dbContext);

        var deleted = await service.DeleteAsync(999);

        Assert.False(deleted);
    }

    private static AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(
                    $"CategoryServiceTests-{Guid.NewGuid()}")
                .Options;

        return new AppDbContext(options);
    }
}
