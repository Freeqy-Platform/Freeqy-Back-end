namespace Freeqy_APIs.Contracts.Projects;

public record SendProjectInvitationRequest(
    string Email
);

public record ProjectInvitationResponse(
    string InviteId,
    string ProjectId,
    string Status,
    string InvitedUser,
    DateTime ExpiresAt,
    string SentBy
);

public record InvitationDetailResponse(
    string InviteId,
    string ProjectId,
    string ProjectName,
    string InvitedEmail,
    string? InvitedUserId,
    string Status,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? RespondedAt,
    string SentByName
);

public record RespondToInvitationRequest(
    string InvitationId,
    bool Accept,
    string? RejectionReason = null
);

public record RespondToInvitationResponse(
    string InviteId,
    string Status,
    string Message
);

public record UserInvitationsResponse(
    IReadOnlyList<InvitationDetailResponse> PendingInvitations,
    IReadOnlyList<InvitationDetailResponse> RespondedInvitations
);
public record ProjectInvitationsResponse(
    IReadOnlyList<InvitationDetailResponse> Invitations
);
