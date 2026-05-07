using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

public class Notification
{
    public string Id { get; set; } = Guid.CreateVersion7().ToString();

    /// <summary>Who receives this notification.</summary>
    public string RecipientId { get; set; } = null!;

    [ForeignKey(nameof(RecipientId))]
    public ApplicationUser Recipient { get; set; } = null!;

    /// <summary>Who triggered this notification (nullable for system notifications).</summary>
    public string? ActorId { get; set; }

    [ForeignKey(nameof(ActorId))]
    public ApplicationUser? Actor { get; set; }

    // Notification content
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    /// <summary>The type of entity this notification refers to (e.g. "Project", "Invitation", "Conversation").</summary>
    public string? EntityType { get; set; }

    /// <summary>The ID of the related entity for deep-linking.</summary>
    public string? EntityId { get; set; }

    /// <summary>Optional full URL for the frontend to navigate to.</summary>
    public string? ActionUrl { get; set; }

    // State
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    /// <summary>Whether an email was sent for this notification.</summary>
    public bool EmailSent { get; set; } = false;
}
