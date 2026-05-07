using Freeqy_APIs.Contracts.Notifications;

namespace Freeqy_APIs.Services;

public interface INotificationService
{
    /// <summary>Send a notification to a single user.</summary>
    Task SendAsync(
        string recipientId,
        string? actorId,
        NotificationType type,
        string title,
        string message,
        string? entityType = null,
        string? entityId = null,
        NotificationPriority priority = NotificationPriority.Normal,
        CancellationToken ct = default);

    /// <summary>Send a notification to multiple users.</summary>
    Task SendToManyAsync(
        IEnumerable<string> recipientIds,
        string? actorId,
        NotificationType type,
        string title,
        string message,
        string? entityType = null,
        string? entityId = null,
        NotificationPriority priority = NotificationPriority.Normal,
        CancellationToken ct = default);

    /// <summary>Get paginated notifications for a user.</summary>
    Task<Result<NotificationListResponse>> GetUserNotificationsAsync(
        string userId,
        int page = 1,
        int pageSize = 20,
        bool? unreadOnly = null,
        CancellationToken ct = default);

    /// <summary>Get the unread notification count for a user.</summary>
    Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);

    /// <summary>Mark a single notification as read.</summary>
    Task<Result> MarkAsReadAsync(string userId, string notificationId, CancellationToken ct = default);

    /// <summary>Mark all notifications as read for a user.</summary>
    Task<Result> MarkAllAsReadAsync(string userId, CancellationToken ct = default);

    /// <summary>Delete a single notification.</summary>
    Task<Result> DeleteNotificationAsync(string userId, string notificationId, CancellationToken ct = default);

    /// <summary>Get notification preferences for a user.</summary>
    Task<Result<NotificationPreferencesListResponse>> GetPreferencesAsync(
        string userId, CancellationToken ct = default);

    /// <summary>Update notification preferences for a user (bulk).</summary>
    Task<Result> UpdatePreferencesAsync(
        string userId,
        BulkUpdateNotificationPreferencesRequest request,
        CancellationToken ct = default);

    /// <summary>Check if a user has a specific notification type enabled for in-app delivery.</summary>
    Task<bool> IsInAppEnabledAsync(string userId, NotificationType type, CancellationToken ct = default);

    /// <summary>Check if a user has a specific notification type enabled for email delivery.</summary>
    Task<bool> IsEmailEnabledAsync(string userId, NotificationType type, CancellationToken ct = default);
}
