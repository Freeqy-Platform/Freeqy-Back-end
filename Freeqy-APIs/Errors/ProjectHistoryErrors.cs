namespace Freeqy_APIs.Errors;

public static class ProjectHistoryErrors
{
    public static readonly Error UserNotFound =
        new("ProjectHistory.UserNotFound", "User not found", StatusCodes.Status404NotFound);
}
