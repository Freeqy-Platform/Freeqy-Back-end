using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Freeqy_APIs.Hubs;

/// <summary>
/// SignalR hub for real-time chat functionality.
/// Supports: messaging, typing indicators, and online presence.
/// Clients connect with JWT bearer token for authentication.
/// </summary>
[Authorize]
public class ChatHub : Hub<IChatClient>
{
    // Thread-safe tracker for online users (userId → set of connectionIds)
    private static readonly ConcurrentDictionary<string, HashSet<string>> OnlineUsers = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is null) return;

        // Track connection
        OnlineUsers.AddOrUpdate(
            userId,
            _ => [Context.ConnectionId],
            (_, connections) =>
            {
                lock (connections) { connections.Add(Context.ConnectionId); }
                return connections;
            });

        // Notify others that this user is online
        await Clients.Others.UserOnline(userId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
        {
            if (OnlineUsers.TryGetValue(userId, out var connections))
            {
                lock (connections) { connections.Remove(Context.ConnectionId); }

                // If no more connections, user is offline
                if (connections.Count == 0)
                {
                    OnlineUsers.TryRemove(userId, out _);
                    await Clients.Others.UserOffline(userId);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client calls this to indicate they are typing in a conversation.
    /// </summary>
    public async Task StartTyping(string conversationId)
    {
        var userId = Context.UserIdentifier;
        if (userId is null) return;

        // We broadcast to all users in the group except the caller.
        // In this simple approach we broadcast to all Others - the client
        // should filter by conversationId.
        await Clients.Others.UserTyping(conversationId, userId, Context.User?.Identity?.Name ?? "");
    }

    /// <summary>
    /// Client calls this to indicate they stopped typing.
    /// </summary>
    public async Task StopTyping(string conversationId)
    {
        var userId = Context.UserIdentifier;
        if (userId is null) return;

        await Clients.Others.UserStoppedTyping(conversationId, userId);
    }

    /// <summary>
    /// Get the list of currently online user IDs.
    /// </summary>
    public Task<List<string>> GetOnlineUsers()
    {
        return Task.FromResult(OnlineUsers.Keys.ToList());
    }
}
