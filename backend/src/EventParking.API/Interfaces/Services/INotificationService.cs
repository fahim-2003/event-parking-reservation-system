using EventParking.API.DTOs.Notifications;

namespace EventParking.API.Interfaces.Services;

public interface INotificationService
{
    Task<NotificationResponse> CreateAsync(
        string userId,
        CreateNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationListResponse>> GetForUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(
        int notificationId,
        string userId,
        CancellationToken cancellationToken = default);
}
