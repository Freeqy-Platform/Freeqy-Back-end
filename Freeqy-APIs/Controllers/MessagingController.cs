using Freeqy_APIs.Contracts.Messaging;
using Microsoft.AspNetCore.RateLimiting;

namespace Freeqy_APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("api")]
public class MessagingController(IMessagingService messagingService) : ControllerBase
{
    private readonly IMessagingService _messagingService = messagingService;

    // ── Conversations ───────────────────────────────────────────────

    /// <summary>Start or resume a direct message conversation.</summary>
    [HttpPost("conversations/dm")]
    public async Task<IActionResult> StartDirectConversation(
        [FromBody] StartDirectConversationRequest request, CancellationToken ct)
    {
        var result = await _messagingService.StartDirectConversationAsync(User.GetUserId()!, request, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Create the default team chat (General channel) for a project.</summary>
    [HttpPost("conversations/team")]
    public async Task<IActionResult> StartTeamConversation(
        [FromBody] StartTeamConversationRequest request, CancellationToken ct)
    {
        var result = await _messagingService.StartTeamConversationAsync(User.GetUserId()!, request, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Get all conversations for the current user.</summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetMyConversations(CancellationToken ct)
    {
        var result = await _messagingService.GetUserConversationsAsync(User.GetUserId()!, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Get a specific conversation by ID.</summary>
    [HttpGet("conversations/{conversationId}")]
    public async Task<IActionResult> GetConversation(string conversationId, CancellationToken ct)
    {
        var result = await _messagingService.GetConversationAsync(User.GetUserId()!, conversationId, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // ── Channels (up to 5 per project) ──────────────────────────────

    /// <summary>Create a new channel for a project (max 5 per project).</summary>
    [HttpPost("channels")]
    public async Task<IActionResult> CreateChannel(
        [FromBody] CreateChannelRequest request, CancellationToken ct)
    {
        var result = await _messagingService.CreateChannelAsync(User.GetUserId()!, request, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Get all channels for a project.</summary>
    [HttpGet("projects/{projectId}/channels")]
    public async Task<IActionResult> GetProjectChannels(string projectId, CancellationToken ct)
    {
        var result = await _messagingService.GetProjectChannelsAsync(User.GetUserId()!, projectId, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Update a channel (rename).</summary>
    [HttpPut("channels/{channelId}")]
    public async Task<IActionResult> UpdateChannel(
        string channelId, [FromBody] UpdateChannelRequest request, CancellationToken ct)
    {
        var result = await _messagingService.UpdateChannelAsync(User.GetUserId()!, channelId, request, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Delete a channel (cannot delete "General").</summary>
    [HttpDelete("channels/{channelId}")]
    public async Task<IActionResult> DeleteChannel(string channelId, CancellationToken ct)
    {
        var result = await _messagingService.DeleteChannelAsync(User.GetUserId()!, channelId, ct);
        return result.IsSuccess ? Ok(new { message = "Channel deleted" }) : result.ToProblem();
    }

    /// <summary>Add members to a channel.</summary>
    [HttpPost("channels/{channelId}/members")]
    public async Task<IActionResult> AddChannelMembers(
        string channelId, [FromBody] AddChannelMembersRequest request, CancellationToken ct)
    {
        var result = await _messagingService.AddChannelMembersAsync(User.GetUserId()!, channelId, request, ct);
        return result.IsSuccess ? Ok(new { message = "Members added" }) : result.ToProblem();
    }

    /// <summary>Remove a member from a channel.</summary>
    [HttpDelete("channels/{channelId}/members/{memberId}")]
    public async Task<IActionResult> RemoveChannelMember(
        string channelId, string memberId, CancellationToken ct)
    {
        var result = await _messagingService.RemoveChannelMemberAsync(User.GetUserId()!, channelId, memberId, ct);
        return result.IsSuccess ? Ok(new { message = "Member removed" }) : result.ToProblem();
    }

    // ── Messages ────────────────────────────────────────────────────

    /// <summary>Send a message in a conversation or channel.</summary>
    [HttpPost("conversations/{conversationId}/messages")]
    [EnableRateLimiting("messaging")]
    public async Task<IActionResult> SendMessage(
        string conversationId, [FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var result = await _messagingService.SendMessageAsync(User.GetUserId()!, conversationId, request, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Get paginated messages for a conversation or channel.</summary>
    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(
        string conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var result = await _messagingService.GetMessagesAsync(User.GetUserId()!, conversationId, page, pageSize, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Edit your own message.</summary>
    [HttpPut("messages/{messageId}")]
    public async Task<IActionResult> EditMessage(
        string messageId, [FromBody] EditMessageRequest request, CancellationToken ct)
    {
        var result = await _messagingService.EditMessageAsync(User.GetUserId()!, messageId, request, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Soft-delete your own message.</summary>
    [HttpDelete("messages/{messageId}")]
    public async Task<IActionResult> DeleteMessage(string messageId, CancellationToken ct)
    {
        var result = await _messagingService.DeleteMessageAsync(User.GetUserId()!, messageId, ct);
        return result.IsSuccess ? Ok(new { message = "Message deleted" }) : result.ToProblem();
    }

    // ── Read Tracking ───────────────────────────────────────────────

    /// <summary>Mark all messages in a conversation as read.</summary>
    [HttpPost("conversations/{conversationId}/read")]
    public async Task<IActionResult> MarkAsRead(string conversationId, CancellationToken ct)
    {
        var result = await _messagingService.MarkConversationAsReadAsync(User.GetUserId()!, conversationId, ct);
        return result.IsSuccess ? Ok(new { message = "Conversation marked as read" }) : result.ToProblem();
    }
}
