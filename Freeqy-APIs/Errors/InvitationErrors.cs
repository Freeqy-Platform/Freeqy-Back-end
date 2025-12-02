namespace Freeqy_APIs.Errors;

public static class InvitationErrors
{
    public static readonly Error NotFound =
        new("Invitation.NotFound", "Invitation not found", StatusCodes.Status404NotFound);

    public static readonly Error AlreadyExists =
        new("Invitation.AlreadyExists", "An invitation already exists for this email", 
            StatusCodes.Status409Conflict);

    public static readonly Error Expired =
        new("Invitation.Expired", "This invitation has expired", StatusCodes.Status410Gone);

    public static readonly Error AlreadyResponded =
        new("Invitation.AlreadyResponded", "You have already responded to this invitation", 
            StatusCodes.Status400BadRequest);

    public static readonly Error Unauthorized =
        new("Invitation.Unauthorized", "Only project owner can send invitations", 
            StatusCodes.Status403Forbidden);

    public static readonly Error SelfInvitation =
        new("Invitation.SelfInvitation", "You cannot invite yourself to the project", 
            StatusCodes.Status400BadRequest);

    public static readonly Error AlreadyMember =
        new("Invitation.AlreadyMember", "User is already a project member", 
            StatusCodes.Status409Conflict);

    public static readonly Error EmailSendFailed =
        new("Invitation.EmailSendFailed", "Failed to send invitation email", 
            StatusCodes.Status500InternalServerError);
}
