namespace Freeqy_APIs.Contracts.Messaging;

public record StartDirectConversationRequest(string RecipientUserId);

public record StartTeamConversationRequest(string ProjectId, string? Title);

public record CreateChannelRequest(string ProjectId, string ChannelName, List<string>? MemberUserIds);

public record UpdateChannelRequest(string? ChannelName);

public record AddChannelMembersRequest(List<string> UserIds);

public record SendMessageRequest(string Content);

public record EditMessageRequest(string Content);
