using Freeqy_APIs.Contracts.Messaging;

namespace Freeqy_APIs.Services;

public interface IMessagingService
{
    // ── Conversations ───────────────────────────────────────────────
    Task<Result<ConversationResponse>> StartDirectConversationAsync(
        string userId, StartDirectConversationRequest request, CancellationToken ct = default);

    Task<Result<ConversationResponse>> StartTeamConversationAsync(
        string userId, StartTeamConversationRequest request, CancellationToken ct = default);

    Task<Result<ConversationListResponse>> GetUserConversationsAsync(
        string userId, CancellationToken ct = default);

    Task<Result<ConversationResponse>> GetConversationAsync(
        string userId, string conversationId, CancellationToken ct = default);

    // ── Channels (per project, max 5) ───────────────────────────────
    Task<Result<ConversationResponse>> CreateChannelAsync(
        string userId, CreateChannelRequest request, CancellationToken ct = default);

    Task<Result<ProjectChannelsResponse>> GetProjectChannelsAsync(
        string userId, string projectId, CancellationToken ct = default);

    Task<Result<ConversationResponse>> UpdateChannelAsync(
        string userId, string channelId, UpdateChannelRequest request, CancellationToken ct = default);

    Task<Result> DeleteChannelAsync(
        string userId, string channelId, CancellationToken ct = default);

    Task<Result> AddChannelMembersAsync(
        string userId, string channelId, AddChannelMembersRequest request, CancellationToken ct = default);

    Task<Result> RemoveChannelMemberAsync(
        string userId, string channelId, string memberId, CancellationToken ct = default);

    // ── Messages ────────────────────────────────────────────────────
    Task<Result<MessageResponse>> SendMessageAsync(
        string userId, string conversationId, SendMessageRequest request, CancellationToken ct = default);

    Task<Result<MessageListResponse>> GetMessagesAsync(
        string userId, string conversationId, int page = 1, int pageSize = 50, CancellationToken ct = default);

    Task<Result<MessageResponse>> EditMessageAsync(
        string userId, string messageId, EditMessageRequest request, CancellationToken ct = default);

    Task<Result> DeleteMessageAsync(
        string userId, string messageId, CancellationToken ct = default);

    // ── Read Tracking ───────────────────────────────────────────────
    Task<Result> MarkConversationAsReadAsync(
        string userId, string conversationId, CancellationToken ct = default);
}
