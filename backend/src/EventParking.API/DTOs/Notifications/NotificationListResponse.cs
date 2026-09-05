namespace EventParking.API.DTOs.Notifications;

public sealed class NotificationListResponse
{
    public int Id { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}
