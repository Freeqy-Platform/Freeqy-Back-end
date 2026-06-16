namespace Freeqy_APIs.Contracts.Messaging;

public record ConversationResponse(
    string Id,
    string Type,
    string? Title,
    string? ChannelName,
    string? ProjectId,
    string? ProjectName,
    DateTime CreatedAt,
    DateTime? LastMessageAt,
    int UnreadCount,
    DateTime? LastReadAt,
    MessageResponse? LastMessage,
    List<ParticipantResponse> Participants
);

public record ParticipantResponse(
    string UserId,
    string Name,
    string? PhotoUrl,
    string Role
);

public record MessageResponse(
    string Id,
    string SenderId,
    string SenderName,
    string? SenderPhotoUrl,
    string Content,
    string Type,
    DateTime CreatedAt,
    DateTime? EditedAt,
    bool IsDeleted,
    List<ReadReceiptResponse>? ReadReceipts = null
);

public record ReadReceiptResponse(
    string UserId,
    string UserName,
    string? UserPhotoUrl,
    DateTime ReadAt
);

public record ConversationListResponse(
    List<ConversationResponse> Conversations,
    int TotalCount,
    int Page,
    int PageSize,
    bool HasMore
);

public record MessageListResponse(
    List<MessageResponse> Messages,
    int TotalCount,
    int Page,
    int PageSize,
    bool HasMore
);

public record ProjectChannelsResponse(
    string ProjectId,
    List<ConversationResponse> Channels,
    int TotalChannels,
    int MaxChannels
);

public record MessageReadReceiptsResponse(
    string MessageId,
    List<ReadReceiptResponse> ReadReceipts
);
