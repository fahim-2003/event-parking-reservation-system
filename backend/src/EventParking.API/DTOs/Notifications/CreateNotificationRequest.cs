namespace EventParking.API.DTOs.Notifications;

public sealed class CreateNotificationRequest
{
    public string Message { get; set; } = string.Empty;
}
