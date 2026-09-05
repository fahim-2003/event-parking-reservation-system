using EventParking.API.Data;
using EventParking.API.DTOs.Notifications;
using EventParking.API.Services;
using Microsoft.EntityFrameworkCore;

namespace EventParking.Tests.Services;

public sealed class NotificationServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesNotification()
    {
        await using var dbContext = CreateDbContext();

        var service = new NotificationService(dbContext);

        var result = await service.CreateAsync(
            "customer-1",
            new CreateNotificationRequest
            {
                Message = "Booking confirmed"
            });

        Assert.Equal(
            "customer-1",
            result.UserId);

        Assert.Equal(
            "Booking confirmed",
            result.Message);

        Assert.False(
            result.IsRead);

        Assert.Single(
            await dbContext.Notifications.ToListAsync());
    }


    [Fact]
    public async Task GetForUserAsync_ReturnsOnlyOwnNotifications()
    {
        await using var dbContext = CreateDbContext();

        dbContext.Notifications.AddRange(
            new EventParking.API.Entities.Notification
            {
                UserId = "customer-1",
                Message = "My notification"
            },
            new EventParking.API.Entities.Notification
            {
                UserId = "customer-2",
                Message = "Other notification"
            });

        await dbContext.SaveChangesAsync();

        var service = new NotificationService(dbContext);

        var result = await service.GetForUserAsync(
            "customer-1");

        Assert.Single(result);

        Assert.Equal(
            "My notification",
            result[0].Message);
    }


    [Fact]
    public async Task MarkAsReadAsync_UpdatesOwnNotification()
    {
        await using var dbContext = CreateDbContext();

        var notification =
            new EventParking.API.Entities.Notification
            {
                UserId = "customer-1",
                Message = "Read me"
            };

        dbContext.Notifications.Add(notification);

        await dbContext.SaveChangesAsync();

        var service = new NotificationService(dbContext);

        var result = await service.MarkAsReadAsync(
            notification.Id,
            "customer-1");

        Assert.True(result);

        var saved =
            await dbContext.Notifications
                .FirstAsync();

        Assert.True(
            saved.IsRead);
    }


    [Fact]
    public async Task MarkAsReadAsync_CannotUpdateOtherUsersNotification()
    {
        await using var dbContext = CreateDbContext();

        var notification =
            new EventParking.API.Entities.Notification
            {
                UserId = "customer-1",
                Message = "Protected"
            };

        dbContext.Notifications.Add(notification);

        await dbContext.SaveChangesAsync();

        var service = new NotificationService(dbContext);

        var result = await service.MarkAsReadAsync(
            notification.Id,
            "customer-2");

        Assert.False(result);
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
