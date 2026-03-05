namespace Freeqy_APIs.Errors;

public static class MessagingErrors
{
    public static readonly Error ConversationNotFound =
        new("Messaging.ConversationNotFound", "Conversation not found.", 404);

    public static readonly Error MessageNotFound =
        new("Messaging.MessageNotFound", "Message not found.", 404);

    public static readonly Error NotAParticipant =
        new("Messaging.NotAParticipant", "You are not a participant in this conversation.", 403);

    public static readonly Error NotMessageOwner =
        new("Messaging.NotMessageOwner", "You can only edit or delete your own messages.", 403);

    public static readonly Error CannotMessageSelf =
        new("Messaging.CannotMessageSelf", "You cannot start a conversation with yourself.", 400);

    public static readonly Error ConversationAlreadyExists =
        new("Messaging.ConversationAlreadyExists", "A direct conversation already exists with this user.", 409);

    public static readonly Error TeamChatAlreadyExists =
        new("Messaging.TeamChatAlreadyExists", "A team chat already exists for this project.", 409);

    public static readonly Error NotProjectMember =
        new("Messaging.NotProjectMember", "You are not a member of this project.", 403);

    public static readonly Error RecipientNotFound =
        new("Messaging.RecipientNotFound", "The recipient user was not found.", 404);

    public static readonly Error ProjectNotFound =
        new("Messaging.ProjectNotFound", "The project was not found.", 404);

    public static readonly Error MessageAlreadyDeleted =
        new("Messaging.MessageAlreadyDeleted", "This message has already been deleted.", 400);

    public static readonly Error MaxChannelsReached =
        new("Messaging.MaxChannelsReached", "This project has reached the maximum of 5 channels.", 400);

    public static readonly Error ChannelNameAlreadyExists =
        new("Messaging.ChannelNameAlreadyExists", "A channel with this name already exists in this project.", 409);

    public static readonly Error CannotDeleteDefaultChannel =
        new("Messaging.CannotDeleteDefaultChannel", "The 'General' channel cannot be deleted.", 400);

    public static readonly Error CannotRenameDefaultChannel =
        new("Messaging.CannotRenameDefaultChannel", "The 'General' channel cannot be renamed.", 400);

    public static readonly Error NotChannelAdmin =
        new("Messaging.NotChannelAdmin", "Only project owners or channel admins can perform this action.", 403);

    public static readonly Error UserAlreadyInChannel =
        new("Messaging.UserAlreadyInChannel", "One or more users are already in this channel.", 409);
}
