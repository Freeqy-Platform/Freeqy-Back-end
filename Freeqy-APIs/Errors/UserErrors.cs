namespace Freeqy_APIs.Errors;

public class UserErrors
{
    public static readonly Error InvalidCredentials =
        new("User.InvalidCredentials", "Invalid email/password", StatusCodes.Status401Unauthorized);

    public static readonly Error DuplicateEmail =
        new("User.DuplicateEmail", "Another user with the same email is already exists", StatusCodes.Status409Conflict);

    public static readonly Error EmailNotConfirmed =
        new("User.EmailNotConfirmed", "Email is not confirmed", StatusCodes.Status401Unauthorized);

    public static readonly Error LockedUser =
        new("User.LockedUser", "Locked user, please contact with administrator", StatusCodes.Status401Unauthorized);
    
    public static readonly Error InvalidToken =
        new("User.InvalidToken", "Invalid token", StatusCodes.Status400BadRequest);
    
    public static readonly Error DuplicateEmailConfirmed = 
        new("User.DuplicateEmailConfirmed", "Another user with the same email is already confirmed", StatusCodes.Status409Conflict);
}