using Freeqy_APIs.Contracts.Messaging;
using Freeqy_APIs.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Freeqy_APIs.Services;

public class MessagingService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IHubContext<ChatHub, IChatClient> hubContext) : IMessagingService
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext = hubContext;

    private const int MaxChannelsPerProject = 5;

    // ═══════════════════════════════════════════════════════════════
    //  CONVERSATIONS
    // ═══════════════════════════════════════════════════════════════

    public async Task<Result<ConversationResponse>> StartDirectConversationAsync(
        string userId, StartDirectConversationRequest request, CancellationToken ct = default)
    {
        if (userId == request.RecipientUserId)
            return Result.Failure<ConversationResponse>(MessagingErrors.CannotMessageSelf);

        var recipient = await _userManager.FindByIdAsync(request.RecipientUserId);
        if (recipient is null)
            return Result.Failure<ConversationResponse>(MessagingErrors.RecipientNotFound);

        // Check for existing DM conversation between the two users
        var existing = await _context.Conversations
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .Where(c => c.Type == ConversationType.DirectMessage)
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .Where(c => c.Participants.Any(p => p.UserId == request.RecipientUserId))
            .FirstOrDefaultAsync(ct);

        if (existing is not null)
            return Result.Success(await BuildConversationResponse(existing, userId, ct));

        var conversation = new Conversation
        {
            Type = ConversationType.DirectMessage,
            CreatedByUserId = userId,
            Participants =
            [
                new ConversationParticipant { UserId = userId, Role = ParticipantRole.Admin },
                new ConversationParticipant { UserId = request.RecipientUserId, Role = ParticipantRole.Member }
            ]
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(ct);

        // Reload with includes
        var created = await _context.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .FirstAsync(c => c.Id == conversation.Id, ct);

        var response = await BuildConversationResponse(created, userId, ct);

        // Notify the recipient via SignalR
        await _hubContext.Clients.User(request.RecipientUserId)
            .ConversationCreated(response);

        return Result.Success(response);
    }

    public async Task<Result<ConversationResponse>> StartTeamConversationAsync(
        string userId, StartTeamConversationRequest request, CancellationToken ct = default)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct);

        if (project is null)
            return Result.Failure<ConversationResponse>(MessagingErrors.ProjectNotFound);

        // Verify the user is a project member or owner
        var isOwner = project.OwnerId == userId;
        var isMember = project.ProjectMembers.Any(pm => pm.UserId == userId && pm.IsActive);
        if (!isOwner && !isMember)
            return Result.Failure<ConversationResponse>(MessagingErrors.NotProjectMember);

        // Check if a "General" channel already exists for this project
        var existing = await _context.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .Where(c => c.Type == ConversationType.ProjectTeam
                        && c.ProjectId == request.ProjectId
                        && c.ChannelName == "General")
            .FirstOrDefaultAsync(ct);

        if (existing is not null)
            return Result.Failure<ConversationResponse>(MessagingErrors.TeamChatAlreadyExists);

        // Build participant list from all active project members + owner
        var participantUserIds = project.ProjectMembers
            .Where(pm => pm.IsActive)
            .Select(pm => pm.UserId)
            .ToHashSet();

        participantUserIds.Add(project.OwnerId);

        var conversation = new Conversation
        {
            Type = ConversationType.ProjectTeam,
            ProjectId = request.ProjectId,
            Title = request.Title ?? project.Name,
            ChannelName = "General",
            CreatedByUserId = userId,
            Participants = participantUserIds.Select(uid => new ConversationParticipant
            {
                UserId = uid,
                Role = uid == project.OwnerId ? ParticipantRole.Admin : ParticipantRole.Member
            }).ToList()
        };

        _context.Conversations.Add(conversation);

        // Post a system message
        var systemMessage = new Message
        {
            ConversationId = conversation.Id,
            SenderId = userId,
            Content = "Team chat created.",
            Type = MessageType.System
        };
        _context.Messages.Add(systemMessage);

        conversation.LastMessageAt = systemMessage.CreatedAt;
        await _context.SaveChangesAsync(ct);

        // Reload with includes
        var created = await _context.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .Include(c => c.Project)
            .FirstAsync(c => c.Id == conversation.Id, ct);

        var response = await BuildConversationResponse(created, userId, ct);

        // Notify all participants via SignalR
        foreach (var participantId in participantUserIds.Where(id => id != userId))
        {
            await _hubContext.Clients.User(participantId).ConversationCreated(response);
        }

        return Result.Success(response);
    }

    public async Task<Result<ConversationListResponse>> GetUserConversationsAsync(
        string userId, CancellationToken ct = default)
    {
        var conversations = await _context.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .Include(c => c.Project)
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync(ct);

        var responses = new List<ConversationResponse>();
        foreach (var conv in conversations)
        {
            responses.Add(await BuildConversationResponse(conv, userId, ct));
        }

        return Result.Success(new ConversationListResponse(responses, responses.Count));
    }

    public async Task<Result<ConversationResponse>> GetConversationAsync(
        string userId, string conversationId, CancellationToken ct = default)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.Id == conversationId, ct);

        if (conversation is null)
            return Result.Failure<ConversationResponse>(MessagingErrors.ConversationNotFound);

        if (!conversation.Participants.Any(p => p.UserId == userId))
            return Result.Failure<ConversationResponse>(MessagingErrors.NotAParticipant);

        return Result.Success(await BuildConversationResponse(conversation, userId, ct));
    }

    // ═══════════════════════════════════════════════════════════════
    //  CHANNELS (up to 5 per project)
    // ═══════════════════════════════════════════════════════════════

    public async Task<Result<ConversationResponse>> CreateChannelAsync(
        string userId, CreateChannelRequest request, CancellationToken ct = default)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct);

        if (project is null)
            return Result.Failure<ConversationResponse>(MessagingErrors.ProjectNotFound);

        // Only project owner or active members can create channels
        var isOwner = project.OwnerId == userId;
        var isMember = project.ProjectMembers.Any(pm => pm.UserId == userId && pm.IsActive);
        if (!isOwner && !isMember)
            return Result.Failure<ConversationResponse>(MessagingErrors.NotProjectMember);

        // Enforce max 5 channels per project
        var existingChannelCount = await _context.Conversations
            .CountAsync(c => c.Type == ConversationType.ProjectTeam && c.ProjectId == request.ProjectId, ct);

        if (existingChannelCount >= MaxChannelsPerProject)
            return Result.Failure<ConversationResponse>(MessagingErrors.MaxChannelsReached);

        // Check for duplicate channel name
        var nameExists = await _context.Conversations
            .AnyAsync(c => c.Type == ConversationType.ProjectTeam
                           && c.ProjectId == request.ProjectId
                           && c.ChannelName == request.ChannelName, ct);

        if (nameExists)
            return Result.Failure<ConversationResponse>(MessagingErrors.ChannelNameAlreadyExists);

        // Build participant list
        var participantUserIds = new HashSet<string> { userId }; // Creator is always included

        if (request.MemberUserIds is not null)
        {
            // Validate that all requested members are active project members or the owner
            var validProjectUserIds = project.ProjectMembers
                .Where(pm => pm.IsActive)
                .Select(pm => pm.UserId)
                .ToHashSet();
            validProjectUserIds.Add(project.OwnerId);

            foreach (var memberId in request.MemberUserIds)
            {
                if (validProjectUserIds.Contains(memberId))
                    participantUserIds.Add(memberId);
            }
        }
        else
        {
            // Default: add all active project members + owner
            foreach (var pm in project.ProjectMembers.Where(pm => pm.IsActive))
                participantUserIds.Add(pm.UserId);
            participantUserIds.Add(project.OwnerId);
        }

        var conversation = new Conversation
        {
            Type = ConversationType.ProjectTeam,
            ProjectId = request.ProjectId,
            Title = request.ChannelName,
            ChannelName = request.ChannelName,
            CreatedByUserId = userId,
            Participants = participantUserIds.Select(uid => new ConversationParticipant
            {
                UserId = uid,
                Role = uid == project.OwnerId || uid == userId ? ParticipantRole.Admin : ParticipantRole.Member
            }).ToList()
        };

        _context.Conversations.Add(conversation);

        var systemMessage = new Message
        {
            ConversationId = conversation.Id,
            SenderId = userId,
            Content = $"Channel '{request.ChannelName}' created.",
            Type = MessageType.System
        };
        _context.Messages.Add(systemMessage);
        conversation.LastMessageAt = systemMessage.CreatedAt;

        await _context.SaveChangesAsync(ct);

        // Reload
        var created = await _context.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .Include(c => c.Project)
            .FirstAsync(c => c.Id == conversation.Id, ct);

        var response = await BuildConversationResponse(created, userId, ct);

        // Notify participants
        foreach (var pid in participantUserIds.Where(id => id != userId))
        {
            await _hubContext.Clients.User(pid).ConversationCreated(response);
        }

        return Result.Success(response);
    }

    public async Task<Result<ProjectChannelsResponse>> GetProjectChannelsAsync(
        string userId, string projectId, CancellationToken ct = default)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);

        if (project is null)
            return Result.Failure<ProjectChannelsResponse>(MessagingErrors.ProjectNotFound);

        var isOwner = project.OwnerId == userId;
        var isMember = project.ProjectMembers.Any(pm => pm.UserId == userId && pm.IsActive);
        if (!isOwner && !isMember)
            return Result.Failure<ProjectChannelsResponse>(MessagingErrors.NotProjectMember);

        var channels = await _context.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .Include(c => c.Project)
            .Where(c => c.Type == ConversationType.ProjectTeam && c.ProjectId == projectId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

        var responses = new List<ConversationResponse>();
        foreach (var ch in channels)
        {
            responses.Add(await BuildConversationResponse(ch, userId, ct));
        }

        return Result.Success(new ProjectChannelsResponse(projectId, responses, responses.Count, MaxChannelsPerProject));
    }

    public async Task<Result<ConversationResponse>> UpdateChannelAsync(
        string userId, string channelId, UpdateChannelRequest request, CancellationToken ct = default)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.Id == channelId && c.Type == ConversationType.ProjectTeam, ct);

        if (conversation is null)
            return Result.Failure<ConversationResponse>(MessagingErrors.ConversationNotFound);

        // Only project owner or channel admin can update
        var project = await _context.Projects.FindAsync([conversation.ProjectId], ct);
        var isOwner = project?.OwnerId == userId;
        var isAdmin = conversation.Participants.Any(p => p.UserId == userId && p.Role == ParticipantRole.Admin);
        if (!isOwner && !isAdmin)
            return Result.Failure<ConversationResponse>(MessagingErrors.NotChannelAdmin);

        // Cannot rename the default "General" channel
        if (conversation.ChannelName == "General" && request.ChannelName is not null)
            return Result.Failure<ConversationResponse>(MessagingErrors.CannotRenameDefaultChannel);

        if (request.ChannelName is not null)
        {
            // Check for duplicate name
            var nameExists = await _context.Conversations
                .AnyAsync(c => c.Type == ConversationType.ProjectTeam
                               && c.ProjectId == conversation.ProjectId
                               && c.ChannelName == request.ChannelName
                               && c.Id != channelId, ct);

            if (nameExists)
                return Result.Failure<ConversationResponse>(MessagingErrors.ChannelNameAlreadyExists);

            var oldName = conversation.ChannelName;
            conversation.ChannelName = request.ChannelName;
            conversation.Title = request.ChannelName;

            var renameMsg = new Message
            {
                ConversationId = channelId,
                SenderId = userId,
                Content = $"Channel renamed from '{oldName}' to '{request.ChannelName}'.",
                Type = MessageType.System
            };
            _context.Messages.Add(renameMsg);
            conversation.LastMessageAt = renameMsg.CreatedAt;
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success(await BuildConversationResponse(conversation, userId, ct));
    }

    public async Task<Result> DeleteChannelAsync(
        string userId, string channelId, CancellationToken ct = default)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Participants)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == channelId && c.Type == ConversationType.ProjectTeam, ct);

        if (conversation is null)
            return Result.Failure(MessagingErrors.ConversationNotFound);

        // Cannot delete the default "General" channel
        if (conversation.ChannelName == "General")
            return Result.Failure(MessagingErrors.CannotDeleteDefaultChannel);

        // Only project owner can delete channels
        var project = await _context.Projects.FindAsync([conversation.ProjectId], ct);
        if (project?.OwnerId != userId)
            return Result.Failure(MessagingErrors.NotChannelAdmin);

        // Notify participants before deletion
        foreach (var p in conversation.Participants.Where(p => p.UserId != userId))
        {
            await _hubContext.Clients.User(p.UserId)
                .MessageDeleted(channelId, "CHANNEL_DELETED");
        }

        _context.Conversations.Remove(conversation);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> AddChannelMembersAsync(
        string userId, string channelId, AddChannelMembersRequest request, CancellationToken ct = default)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == channelId && c.Type == ConversationType.ProjectTeam, ct);

        if (conversation is null)
            return Result.Failure(MessagingErrors.ConversationNotFound);

        // Only project owner or channel admin can add members
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == conversation.ProjectId, ct);

        var isOwner = project?.OwnerId == userId;
        var isAdmin = conversation.Participants.Any(p => p.UserId == userId && p.Role == ParticipantRole.Admin);
        if (!isOwner && !isAdmin)
            return Result.Failure(MessagingErrors.NotChannelAdmin);

        // Validate all user IDs are active project members
        var validProjectUserIds = project!.ProjectMembers
            .Where(pm => pm.IsActive)
            .Select(pm => pm.UserId)
            .ToHashSet();
        validProjectUserIds.Add(project.OwnerId);

        var existingParticipantIds = conversation.Participants.Select(p => p.UserId).ToHashSet();
        var addedUsers = new List<string>();

        foreach (var memberId in request.UserIds)
        {
            if (!validProjectUserIds.Contains(memberId))
                continue; // Skip non-project members silently

            if (existingParticipantIds.Contains(memberId))
                continue; // Skip already-in-channel members

            _context.ConversationParticipants.Add(new ConversationParticipant
            {
                ConversationId = channelId,
                UserId = memberId,
                Role = ParticipantRole.Member
            });
            addedUsers.Add(memberId);
        }

        if (addedUsers.Count > 0)
        {
            var addedNames = new List<string>();
            foreach (var uid in addedUsers)
            {
                var user = await _userManager.FindByIdAsync(uid);
                if (user is not null) addedNames.Add($"{user.FirstName} {user.LastName}");
            }

            var systemMsg = new Message
            {
                ConversationId = channelId,
                SenderId = userId,
                Content = $"{string.Join(", ", addedNames)} added to the channel.",
                Type = MessageType.System
            };
            _context.Messages.Add(systemMsg);
            conversation.LastMessageAt = systemMsg.CreatedAt;

            await _context.SaveChangesAsync(ct);

            // Notify new members
            foreach (var uid in addedUsers)
            {
                var conv = await _context.Conversations
                    .Include(c => c.Participants).ThenInclude(p => p.User)
                    .Include(c => c.Project)
                    .FirstAsync(c => c.Id == channelId, ct);
                await _hubContext.Clients.User(uid)
                    .ConversationCreated(await BuildConversationResponse(conv, uid, ct));
            }
        }

        return Result.Success();
    }

    public async Task<Result> RemoveChannelMemberAsync(
        string userId, string channelId, string memberId, CancellationToken ct = default)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == channelId && c.Type == ConversationType.ProjectTeam, ct);

        if (conversation is null)
            return Result.Failure(MessagingErrors.ConversationNotFound);

        var project = await _context.Projects.FindAsync([conversation.ProjectId], ct);
        var isOwner = project?.OwnerId == userId;
        var isAdmin = conversation.Participants.Any(p => p.UserId == userId && p.Role == ParticipantRole.Admin);
        if (!isOwner && !isAdmin)
            return Result.Failure(MessagingErrors.NotChannelAdmin);

        var participant = conversation.Participants.FirstOrDefault(p => p.UserId == memberId);
        if (participant is null)
            return Result.Failure(MessagingErrors.NotAParticipant);

        _context.ConversationParticipants.Remove(participant);

        var removedUser = await _userManager.FindByIdAsync(memberId);
        var systemMsg = new Message
        {
            ConversationId = channelId,
            SenderId = userId,
            Content = $"{removedUser?.FirstName} {removedUser?.LastName} was removed from the channel.",
            Type = MessageType.System
        };
        _context.Messages.Add(systemMsg);
        conversation.LastMessageAt = systemMsg.CreatedAt;

        await _context.SaveChangesAsync(ct);

        // Notify remaining participants
        foreach (var p in conversation.Participants.Where(p => p.UserId != memberId))
        {
            await _hubContext.Clients.User(p.UserId)
                .ReceiveMessage(channelId, new MessageResponse(
                    systemMsg.Id, systemMsg.SenderId, "System", null,
                    systemMsg.Content, systemMsg.Type.ToString(),
                    systemMsg.CreatedAt, null, false));
        }

        return Result.Success();
    }

    // ═══════════════════════════════════════════════════════════════
    //  MESSAGES
    // ═══════════════════════════════════════════════════════════════

    public async Task<Result<MessageResponse>> SendMessageAsync(
        string userId, string conversationId, SendMessageRequest request, CancellationToken ct = default)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conversationId, ct);

        if (conversation is null)
            return Result.Failure<MessageResponse>(MessagingErrors.ConversationNotFound);

        if (!conversation.Participants.Any(p => p.UserId == userId))
            return Result.Failure<MessageResponse>(MessagingErrors.NotAParticipant);

        var sender = await _userManager.FindByIdAsync(userId);

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = userId,
            Content = request.Content,
            Type = MessageType.Text
        };

        _context.Messages.Add(message);
        conversation.LastMessageAt = message.CreatedAt;
        await _context.SaveChangesAsync(ct);

        var response = new MessageResponse(
            message.Id,
            userId,
            $"{sender!.FirstName} {sender.LastName}",
            sender.PhotoUrl,
            message.Content,
            message.Type.ToString(),
            message.CreatedAt,
            message.EditedAt,
            message.IsDeleted
        );

        // Push to all participants via SignalR
        var recipientIds = conversation.Participants
            .Where(p => p.UserId != userId)
            .Select(p => p.UserId)
            .ToList();

        foreach (var recipientId in recipientIds)
        {
            await _hubContext.Clients.User(recipientId)
                .ReceiveMessage(conversationId, response);
        }

        return Result.Success(response);
    }

    public async Task<Result<MessageListResponse>> GetMessagesAsync(
        string userId, string conversationId, int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        var isParticipant = await _context.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId, ct);

        if (!isParticipant)
            return Result.Failure<MessageListResponse>(MessagingErrors.NotAParticipant);

        var query = _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync(ct);

        var messages = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MessageResponse(
                m.Id,
                m.SenderId,
                m.Sender.FirstName + " " + m.Sender.LastName,
                m.Sender.PhotoUrl,
                m.IsDeleted ? "" : m.Content,
                m.Type.ToString(),
                m.CreatedAt,
                m.EditedAt,
                m.IsDeleted
            ))
            .ToListAsync(ct);

        var hasMore = (page * pageSize) < totalCount;

        return Result.Success(new MessageListResponse(messages, totalCount, page, pageSize, hasMore));
    }

    public async Task<Result<MessageResponse>> EditMessageAsync(
        string userId, string messageId, EditMessageRequest request, CancellationToken ct = default)
    {
        var message = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Conversation).ThenInclude(c => c.Participants)
            .FirstOrDefaultAsync(m => m.Id == messageId, ct);

        if (message is null)
            return Result.Failure<MessageResponse>(MessagingErrors.MessageNotFound);

        if (message.SenderId != userId)
            return Result.Failure<MessageResponse>(MessagingErrors.NotMessageOwner);

        if (message.IsDeleted)
            return Result.Failure<MessageResponse>(MessagingErrors.MessageAlreadyDeleted);

        message.Content = request.Content;
        message.EditedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        var response = new MessageResponse(
            message.Id,
            message.SenderId,
            $"{message.Sender.FirstName} {message.Sender.LastName}",
            message.Sender.PhotoUrl,
            message.Content,
            message.Type.ToString(),
            message.CreatedAt,
            message.EditedAt,
            message.IsDeleted
        );

        // Notify participants about the edit
        var recipientIds = message.Conversation.Participants
            .Where(p => p.UserId != userId)
            .Select(p => p.UserId);

        foreach (var recipientId in recipientIds)
        {
            await _hubContext.Clients.User(recipientId)
                .MessageEdited(message.ConversationId, response);
        }

        return Result.Success(response);
    }

    public async Task<Result> DeleteMessageAsync(
        string userId, string messageId, CancellationToken ct = default)
    {
        var message = await _context.Messages
            .Include(m => m.Conversation).ThenInclude(c => c.Participants)
            .FirstOrDefaultAsync(m => m.Id == messageId, ct);

        if (message is null)
            return Result.Failure(MessagingErrors.MessageNotFound);

        if (message.SenderId != userId)
            return Result.Failure(MessagingErrors.NotMessageOwner);

        if (message.IsDeleted)
            return Result.Failure(MessagingErrors.MessageAlreadyDeleted);

        message.IsDeleted = true;
        message.Content = string.Empty;
        await _context.SaveChangesAsync(ct);

        // Notify participants about the deletion
        var recipientIds = message.Conversation.Participants
            .Where(p => p.UserId != userId)
            .Select(p => p.UserId);

        foreach (var recipientId in recipientIds)
        {
            await _hubContext.Clients.User(recipientId)
                .MessageDeleted(message.ConversationId, messageId);
        }

        return Result.Success();
    }

    // ═══════════════════════════════════════════════════════════════
    //  READ TRACKING
    // ═══════════════════════════════════════════════════════════════

    public async Task<Result> MarkConversationAsReadAsync(
        string userId, string conversationId, CancellationToken ct = default)
    {
        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId, ct);

        if (participant is null)
            return Result.Failure(MessagingErrors.NotAParticipant);

        participant.LastReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        // Notify other participants about read receipt
        var otherParticipantIds = await _context.ConversationParticipants
            .Where(cp => cp.ConversationId == conversationId && cp.UserId != userId)
            .Select(cp => cp.UserId)
            .ToListAsync(ct);

        foreach (var otherUserId in otherParticipantIds)
        {
            await _hubContext.Clients.User(otherUserId)
                .ConversationRead(conversationId, userId);
        }

        return Result.Success();
    }

    // ═══════════════════════════════════════════════════════════════
    //  PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════

    private async Task<ConversationResponse> BuildConversationResponse(
        Conversation conversation, string currentUserId, CancellationToken ct)
    {
        var participant = conversation.Participants.FirstOrDefault(p => p.UserId == currentUserId);

        // Unread count
        var unreadCount = 0;
        if (participant is not null)
        {
            var lastRead = participant.LastReadAt ?? DateTime.MinValue;
            unreadCount = await _context.Messages
                .CountAsync(m => m.ConversationId == conversation.Id
                                 && m.CreatedAt > lastRead
                                 && m.SenderId != currentUserId, ct);
        }

        // Last message
        var lastMsg = await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversation.Id)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync(ct);

        MessageResponse? lastMessageResponse = null;
        if (lastMsg is not null)
        {
            lastMessageResponse = new MessageResponse(
                lastMsg.Id,
                lastMsg.SenderId,
                $"{lastMsg.Sender.FirstName} {lastMsg.Sender.LastName}",
                lastMsg.Sender.PhotoUrl,
                lastMsg.IsDeleted ? "" : lastMsg.Content,
                lastMsg.Type.ToString(),
                lastMsg.CreatedAt,
                lastMsg.EditedAt,
                lastMsg.IsDeleted
            );
        }

        var participants = conversation.Participants.Select(p => new ParticipantResponse(
            p.UserId,
            $"{p.User.FirstName} {p.User.LastName}",
            p.User.PhotoUrl,
            p.Role.ToString()
        )).ToList();

        return new ConversationResponse(
            conversation.Id,
            conversation.Type.ToString(),
            conversation.Title,
            conversation.ChannelName,
            conversation.ProjectId,
            conversation.Project?.Name,
            conversation.CreatedAt,
            conversation.LastMessageAt,
            unreadCount,
            lastMessageResponse,
            participants
        );
    }
}
