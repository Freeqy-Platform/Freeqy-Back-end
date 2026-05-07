using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

/// <summary>
/// Per-user, per-notification-type preferences for delivery channels.
/// If no preference row exists for a given type, defaults apply (InApp=true, Email=true).
/// </summary>
public class NotificationPreference
{
    public string UserId { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    public NotificationType Type { get; set; }

    /// <summary>Whether in-app (SignalR + notification center) is enabled for this type.</summary>
    public bool InAppEnabled { get; set; } = true;

    /// <summary>Whether email delivery is enabled for this type.</summary>
    public bool EmailEnabled { get; set; } = true;
}
