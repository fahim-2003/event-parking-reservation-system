using EventParking.API.Data;
using EventParking.API.DTOs.Notifications;
using EventParking.API.Entities;
using EventParking.API.Services;
using Microsoft.EntityFrameworkCore;

namespace EventParking.Tests.Services;

public sealed class NotificationServiceTests
{
    [Fact]
    public async Task GetForUserAsync_ReturnsOnlyRequestedUsersNotifications()
    {
        await using var db = CreateDbContext();

        db.Notifications.AddRange(
            new Notification
            {
                UserId = "customer-1",
                Message = "Customer one notification",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            },
            new Notification
            {
                UserId = "customer-2",
                Message = "Customer two notification",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync();

        var service = new NotificationService(db);

        var result =
            await service.GetForUserAsync(
                "customer-1");

        Assert.Single(result);

        Assert.Equal(
            "Customer one notification",
            result[0].Message);
    }

    [Fact]
    public async Task MarkAsReadAsync_CannotModifyAnotherUsersNotification()
    {
        await using var db = CreateDbContext();

        var notification =
            new Notification
            {
                UserId = "customer-2",
                Message = "Private notification",
                IsRead = false
            };

        db.Notifications.Add(notification);

        await db.SaveChangesAsync();

        var service = new NotificationService(db);

        var result =
            await service.MarkAsReadAsync(
                notification.Id,
                "customer-1");

        Assert.False(result);

        var stored =
            await db.Notifications
                .SingleAsync();

        Assert.False(stored.IsRead);
    }

    [Fact]
    public async Task MarkAsReadAsync_MarksOwnedNotificationAsRead()
    {
        await using var db = CreateDbContext();

        var notification =
            new Notification
            {
                UserId = "customer-1",
                Message = "Read me",
                IsRead = false
            };

        db.Notifications.Add(notification);

        await db.SaveChangesAsync();

        var service = new NotificationService(db);

        var result =
            await service.MarkAsReadAsync(
                notification.Id,
                "customer-1");

        Assert.True(result);

        var stored =
            await db.Notifications
                .SingleAsync();

        Assert.True(stored.IsRead);
    }

    [Fact]
    public async Task CreateAsync_CreatesUnreadNotificationForSpecifiedUser()
    {
        await using var db = CreateDbContext();

        var service = new NotificationService(db);

        var result =
            await service.CreateAsync(
                "customer-1",
                new CreateNotificationRequest
                {
                    Message = "Booking update"
                });

        Assert.Equal(
            "customer-1",
            result.UserId);

        Assert.Equal(
            "Booking update",
            result.Message);

        Assert.False(result.IsRead);

        var stored =
            await db.Notifications
                .SingleAsync();

        Assert.Equal(
            "customer-1",
            stored.UserId);

        Assert.False(stored.IsRead);
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
