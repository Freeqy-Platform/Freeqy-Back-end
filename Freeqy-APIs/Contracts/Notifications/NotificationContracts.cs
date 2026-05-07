namespace Freeqy_APIs.Contracts.Notifications;

public record NotificationResponse(
    string Id,
    string Type,
    string Priority,
    string Title,
    string Message,
    string? ActorId,
    string? ActorName,
    string? ActorPhotoUrl,
    string? EntityType,
    string? EntityId,
    string? ActionUrl,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt
);

public record NotificationListResponse(
    List<NotificationResponse> Notifications,
    int TotalCount,
    int UnreadCount,
    int Page,
    int PageSize,
    bool HasMore
);

public record UnreadCountResponse(int UnreadCount);

public record NotificationPreferenceResponse(
    string Type,
    bool InAppEnabled,
    bool EmailEnabled
);

public record NotificationPreferencesListResponse(
    List<NotificationPreferenceResponse> Preferences
);

public record UpdateNotificationPreferenceRequest(
    string Type,
    bool InAppEnabled,
    bool EmailEnabled
);

public record BulkUpdateNotificationPreferencesRequest(
    List<UpdateNotificationPreferenceRequest> Preferences
);
