using Freeqy_APIs.Contracts.Notifications;
using Freeqy_APIs.Hubs;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.SignalR;

namespace Freeqy_APIs.Services;

public class NotificationService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IHubContext<ChatHub, IChatClient> hubContext,
    IEmailSender emailSender,
    ILogger<NotificationService> logger) : INotificationService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext = hubContext;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ILogger<NotificationService> _logger = logger;

    // Notification types that trigger email by default
    private static readonly HashSet<NotificationType> EmailDefaultTypes =
    [
        NotificationType.InvitationReceived,
        NotificationType.BadgeEarned,
        NotificationType.SecurityAlert
    ];

    // ═══════════════════════════════════════════════════════════════
    //  SEND
    // ═══════════════════════════════════════════════════════════════

    public async Task SendAsync(
        string recipientId,
        string? actorId,
        NotificationType type,
        string title,
        string message,
        string? entityType = null,
        string? entityId = null,
        NotificationPriority priority = NotificationPriority.Normal,
        CancellationToken ct = default)
    {
        // Check if in-app is enabled for this user + type
        var inAppEnabled = await IsInAppEnabledAsync(recipientId, type, ct);
        if (!inAppEnabled) return;

        // Deduplication: don't create a duplicate unread notification of the same type for the same entity
        if (entityId is not null && type == NotificationType.NewMessage)
        {
            var hasDuplicate = await _dbContext.Notifications
                .AnyAsync(n => n.RecipientId == recipientId
                            && n.Type == type
                            && n.EntityId == entityId
                            && !n.IsRead, ct);

            if (hasDuplicate) return;
        }

        var notification = new Notification
        {
            RecipientId = recipientId,
            ActorId = actorId,
            Type = type,
            Priority = priority,
            Title = title,
            Message = message,
            EntityType = entityType,
            EntityId = entityId
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(ct);

        // Build the response DTO
        var actor = actorId is not null ? await _userManager.FindByIdAsync(actorId) : null;
        var response = MapToResponse(notification, actor);

        // Push real-time via SignalR
        try
        {
            await _hubContext.Clients.User(recipientId).ReceiveNotification(response);
            var unreadCount = await GetUnreadCountAsync(recipientId, ct);
            await _hubContext.Clients.User(recipientId).UnreadCountUpdated(unreadCount);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to push notification via SignalR to user {UserId}", recipientId);
        }

        // Send email if applicable
        await TrySendEmailAsync(recipientId, type, title, message, ct);
    }

    public async Task SendToManyAsync(
        IEnumerable<string> recipientIds,
        string? actorId,
        NotificationType type,
        string title,
        string message,
        string? entityType = null,
        string? entityId = null,
        NotificationPriority priority = NotificationPriority.Normal,
        CancellationToken ct = default)
    {
        foreach (var recipientId in recipientIds.Distinct())
        {
            if (ct.IsCancellationRequested) break;
            await SendAsync(recipientId, actorId, type, title, message, entityType, entityId, priority, ct);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  QUERY
    // ═══════════════════════════════════════════════════════════════

    public async Task<Result<NotificationListResponse>> GetUserNotificationsAsync(
        string userId,
        int page = 1,
        int pageSize = 20,
        bool? unreadOnly = null,
        CancellationToken ct = default)
    {
        var query = _dbContext.Notifications
            .Include(n => n.Actor)
            .Where(n => n.RecipientId == userId)
            .AsNoTracking();

        if (unreadOnly == true)
            query = query.Where(n => !n.IsRead);

        var totalCount = await query.CountAsync(ct);
        var unreadCount = await _dbContext.Notifications
            .CountAsync(n => n.RecipientId == userId && !n.IsRead, ct);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = notifications.Select(n => MapToResponse(n, n.Actor)).ToList();

        var hasMore = (page * pageSize) < totalCount;

        return Result.Success(new NotificationListResponse(
            responses, totalCount, unreadCount, page, pageSize, hasMore));
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .CountAsync(n => n.RecipientId == userId && !n.IsRead, ct);
    }

    // ═══════════════════════════════════════════════════════════════
    //  MARK READ / DELETE
    // ═══════════════════════════════════════════════════════════════

    public async Task<Result> MarkAsReadAsync(string userId, string notificationId, CancellationToken ct = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, ct);

        if (notification is null)
            return Result.Failure(NotificationErrors.NotFound);

        if (notification.RecipientId != userId)
            return Result.Failure(NotificationErrors.Unauthorized);

        if (notification.IsRead)
            return Result.Success(); // Idempotent — no error

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);

        // Push updated unread count
        var unreadCount = await GetUnreadCountAsync(userId, ct);
        await _hubContext.Clients.User(userId).UnreadCountUpdated(unreadCount);

        return Result.Success();
    }

    public async Task<Result> MarkAllAsReadAsync(string userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        await _dbContext.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now), ct);

        // Push updated unread count (should be 0 now)
        await _hubContext.Clients.User(userId).UnreadCountUpdated(0);

        return Result.Success();
    }

    public async Task<Result> DeleteNotificationAsync(string userId, string notificationId, CancellationToken ct = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, ct);

        if (notification is null)
            return Result.Failure(NotificationErrors.NotFound);

        if (notification.RecipientId != userId)
            return Result.Failure(NotificationErrors.Unauthorized);

        _dbContext.Notifications.Remove(notification);
        await _dbContext.SaveChangesAsync(ct);

        // Push updated unread count if it was unread
        if (!notification.IsRead)
        {
            var unreadCount = await GetUnreadCountAsync(userId, ct);
            await _hubContext.Clients.User(userId).UnreadCountUpdated(unreadCount);
        }

        return Result.Success();
    }

    // ═══════════════════════════════════════════════════════════════
    //  PREFERENCES
    // ═══════════════════════════════════════════════════════════════

    public async Task<Result<NotificationPreferencesListResponse>> GetPreferencesAsync(
        string userId, CancellationToken ct = default)
    {
        var saved = await _dbContext.NotificationPreferences
            .Where(np => np.UserId == userId)
            .AsNoTracking()
            .ToListAsync(ct);

        // Build a complete list for all notification types, using saved values or defaults
        var allTypes = Enum.GetValues<NotificationType>();
        var savedDict = saved.ToDictionary(np => np.Type);

        var preferences = allTypes.Select(type =>
        {
            if (savedDict.TryGetValue(type, out var pref))
                return new NotificationPreferenceResponse(type.ToString(), pref.InAppEnabled, pref.EmailEnabled);

            // Default: InApp always on, Email only for specific types
            return new NotificationPreferenceResponse(
                type.ToString(),
                InAppEnabled: true,
                EmailEnabled: EmailDefaultTypes.Contains(type));
        }).ToList();

        return Result.Success(new NotificationPreferencesListResponse(preferences));
    }

    public async Task<Result> UpdatePreferencesAsync(
        string userId,
        BulkUpdateNotificationPreferencesRequest request,
        CancellationToken ct = default)
    {
        foreach (var item in request.Preferences)
        {
            if (!Enum.TryParse<NotificationType>(item.Type, out var type))
                return Result.Failure(NotificationErrors.InvalidType);

            var existing = await _dbContext.NotificationPreferences
                .FirstOrDefaultAsync(np => np.UserId == userId && np.Type == type, ct);

            if (existing is not null)
            {
                existing.InAppEnabled = item.InAppEnabled;
                existing.EmailEnabled = item.EmailEnabled;
            }
            else
            {
                _dbContext.NotificationPreferences.Add(new NotificationPreference
                {
                    UserId = userId,
                    Type = type,
                    InAppEnabled = item.InAppEnabled,
                    EmailEnabled = item.EmailEnabled
                });
            }
        }

        await _dbContext.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<bool> IsInAppEnabledAsync(string userId, NotificationType type, CancellationToken ct = default)
    {
        var pref = await _dbContext.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == userId && np.Type == type, ct);

        return pref?.InAppEnabled ?? true; // Default: enabled
    }

    public async Task<bool> IsEmailEnabledAsync(string userId, NotificationType type, CancellationToken ct = default)
    {
        var pref = await _dbContext.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == userId && np.Type == type, ct);

        return pref?.EmailEnabled ?? EmailDefaultTypes.Contains(type);
    }

    // ═══════════════════════════════════════════════════════════════
    //  PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════

    private async Task TrySendEmailAsync(
        string recipientId,
        NotificationType type,
        string title,
        string body,
        CancellationToken ct)
    {
        try
        {
            var emailEnabled = await IsEmailEnabledAsync(recipientId, type, ct);
            if (!emailEnabled) return;

            var user = await _userManager.FindByIdAsync(recipientId);
            if (user?.Email is null) return;

            await _emailSender.SendEmailAsync(user.Email, title, body);

            // Mark email as sent on the most recent notification
            var notification = await _dbContext.Notifications
                .Where(n => n.RecipientId == recipientId && n.Type == type)
                .OrderByDescending(n => n.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (notification is not null)
            {
                notification.EmailSent = true;
                await _dbContext.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notification email to user {UserId} for type {Type}",
                recipientId, type);
        }
    }

    private static NotificationResponse MapToResponse(Notification n, ApplicationUser? actor)
    {
        return new NotificationResponse(
            Id: n.Id,
            Type: n.Type.ToString(),
            Priority: n.Priority.ToString(),
            Title: n.Title,
            Message: n.Message,
            ActorId: n.ActorId,
            ActorName: actor is not null ? $"{actor.FirstName} {actor.LastName}" : null,
            ActorPhotoUrl: actor?.PhotoUrl,
            EntityType: n.EntityType,
            EntityId: n.EntityId,
            ActionUrl: n.ActionUrl,
            IsRead: n.IsRead,
            CreatedAt: n.CreatedAt,
            ReadAt: n.ReadAt
        );
    }
}
