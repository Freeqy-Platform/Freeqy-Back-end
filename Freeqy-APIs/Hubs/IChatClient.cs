using Freeqy_APIs.Contracts.Messaging;

namespace Freeqy_APIs.Hubs;

/// <summary>
/// Strongly-typed SignalR client interface for the chat hub.
/// Defines methods the server can invoke on connected clients.
/// </summary>
public interface IChatClient
{
    /// <summary>Called when a new message is received in a conversation.</summary>
    Task ReceiveMessage(string conversationId, MessageResponse message);

    /// <summary>Called when a message is edited.</summary>
    Task MessageEdited(string conversationId, MessageResponse message);

    /// <summary>Called when a message is deleted.</summary>
    Task MessageDeleted(string conversationId, string messageId);

    /// <summary>Called when a new conversation is created that includes this user.</summary>
    Task ConversationCreated(ConversationResponse conversation);

    /// <summary>Called when a user reads a conversation (read receipt).</summary>
    Task ConversationRead(string conversationId, string userId);

    /// <summary>Called when a user starts typing in a conversation.</summary>
    Task UserTyping(string conversationId, string userId, string userName);

    /// <summary>Called when a user stops typing.</summary>
    Task UserStoppedTyping(string conversationId, string userId);

    /// <summary>Called when a user comes online.</summary>
    Task UserOnline(string userId);

    /// <summary>Called when a user goes offline.</summary>
    Task UserOffline(string userId);
}
