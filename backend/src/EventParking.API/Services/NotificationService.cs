using EventParking.API.Data;
using EventParking.API.DTOs.Notifications;
using EventParking.API.Entities;
using EventParking.API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Services;

public sealed class NotificationService : INotificationService
{
    private readonly AppDbContext _dbContext;

    public NotificationService(
        AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotificationResponse> CreateAsync(
        string userId,
        CreateNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = request.Message,
            IsRead = false
        };

        _dbContext.Notifications.Add(notification);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new NotificationResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }

    public async Task<IReadOnlyList<NotificationListResponse>> GetForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .Where(notification =>
                notification.UserId == userId)
            .OrderByDescending(notification =>
                notification.CreatedAt)
            .Select(notification => new NotificationListResponse
            {
                Id = notification.Id,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(
        int notificationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(
                n =>
                    n.Id == notificationId &&
                    n.UserId == userId,
                cancellationToken);

        if (notification is null)
        {
            return false;
        }

        notification.IsRead = true;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
