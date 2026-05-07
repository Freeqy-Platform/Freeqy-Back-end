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

    public static readonly Error InvalidMeetingType =
        new("Meeting.InvalidType", "Invalid meeting type", StatusCodes.Status400BadRequest);

    public static readonly Error LocationRequiredForOfflineMeeting =
        new("Meeting.LocationRequired", "Location is required for offline meetings", StatusCodes.Status400BadRequest);

    public static readonly Error CannotDeleteMeetingNotCreatedByUser =
        new("Meeting.CannotDelete", "Only the user who created the meeting can delete it", StatusCodes.Status403Forbidden);

    public static readonly Error CannotUpdateMeetingNotCreatedByUser =
        new("Meeting.CannotUpdate", "Only the user who created the meeting can update it", StatusCodes.Status403Forbidden);
}

