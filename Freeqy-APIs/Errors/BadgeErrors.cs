namespace Freeqy_APIs.Errors;

public static class BadgeErrors
{
    public static readonly Error UserNotFound =
        new("Badge.UserNotFound", "User not found", StatusCodes.Status404NotFound);
}
