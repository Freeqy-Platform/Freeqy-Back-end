namespace Freeqy_APIs.Entities;

public enum NotificationType
{
    // Invitations
    InvitationReceived,
    InvitationAccepted,
    InvitationRejected,
    InvitationExpired,
    InvitationCancelled,

    // Projects
    ProjectStatusChanged,
    ProjectUpdated,
    ProjectDeleted,

    // Team
    MemberJoined,
    MemberLeft,
    MemberRemoved,
    MemberRoleChanged,

    // Messages
    NewMessage,
    MentionedInMessage,
    AddedToConversation,

    // Badges
    BadgeEarned,

    // Track Requests
    TrackRequestApproved,
    TrackRequestRejected,

    // System
    SystemAnnouncement,
    SecurityAlert
}
