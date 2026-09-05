using System.Security.Claims;
using EventParking.API.DTOs.Notifications;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/customer/notifications")]
[Authorize(Roles = "Customer")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationListResponse>>> Get(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var notifications =
            await _notificationService.GetForUserAsync(
                userId,
                cancellationToken);

        return Ok(notifications);
    }


    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var updated =
            await _notificationService.MarkAsReadAsync(
                id,
                userId,
                cancellationToken);

        return updated
            ? NoContent()
            : NotFound();
    }
}
