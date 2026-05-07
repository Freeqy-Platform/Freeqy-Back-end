using Freeqy_APIs.Contracts.Notifications;

namespace Freeqy_APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    private readonly INotificationService _notificationService = notificationService;

    /// <summary>Get paginated notifications for the current user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? unreadOnly = null,
        CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize, unreadOnly, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>Get the unread notification count for the current user.</summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var count = await _notificationService.GetUnreadCountAsync(userId, ct);

        return Ok(new UnreadCountResponse(count));
    }

    /// <summary>Mark a specific notification as read.</summary>
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _notificationService.MarkAsReadAsync(userId, id, ct);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>Mark all notifications as read for the current user.</summary>
    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _notificationService.MarkAllAsReadAsync(userId, ct);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>Delete a specific notification.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(string id, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _notificationService.DeleteNotificationAsync(userId, id, ct);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    // ═══════════════════════════════════════════════════════════════
    //  PREFERENCES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Get notification preferences for the current user.</summary>
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _notificationService.GetPreferencesAsync(userId, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>Update notification preferences for the current user (bulk).</summary>
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] BulkUpdateNotificationPreferencesRequest request,
        CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _notificationService.UpdatePreferencesAsync(userId, request, ct);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
