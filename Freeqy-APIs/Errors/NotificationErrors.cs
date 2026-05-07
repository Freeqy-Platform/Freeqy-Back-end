namespace Freeqy_APIs.Errors;

public static class NotificationErrors
{
    public static readonly Error NotFound =
        new("Notification.NotFound", "Notification not found", StatusCodes.Status404NotFound);

    public static readonly Error Unauthorized =
        new("Notification.Unauthorized", "You do not have access to this notification",
            StatusCodes.Status403Forbidden);

    public static readonly Error InvalidType =
        new("Notification.InvalidType", "Invalid notification type specified",
            StatusCodes.Status400BadRequest);

    public static readonly Error AlreadyRead =
        new("Notification.AlreadyRead", "Notification is already marked as read",
            StatusCodes.Status400BadRequest);
}
