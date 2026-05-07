namespace Freeqy_APIs.Errors;

public static class MeetingErrors
{
    public static readonly Error MeetingNotFound =
        new("Meeting.NotFound", "Meeting not found", StatusCodes.Status404NotFound);

    public static readonly Error MeetingNotFoundForProject =
        new("Meeting.NotFoundForProject", "No meeting found for this project", StatusCodes.Status404NotFound);

    public static readonly Error ProjectNotFound =
        new("Meeting.ProjectNotFound", "Project not found", StatusCodes.Status404NotFound);

    public static readonly Error Unauthorized =
        new("Meeting.Unauthorized", "You are not authorized to perform this action", StatusCodes.Status403Forbidden);

    public static readonly Error MeetingAlreadyDeleted =
        new("Meeting.AlreadyDeleted", "Meeting is already deleted", StatusCodes.Status400BadRequest);
}
