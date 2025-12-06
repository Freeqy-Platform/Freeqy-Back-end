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

    public static readonly Error UserNotFound =
        new("User.UserNotFound", "User not found", StatusCodes.Status404NotFound);

    public static readonly Error PhotoNotFound =
        new("User.PhotoNotFound", "User photo not found", StatusCodes.Status404NotFound);

    public static readonly Error InvalidPhotoFile =
        new("User.InvalidPhotoFile", "Invalid photo file. Please upload a valid image (JPEG, PNG, or WebP).", StatusCodes.Status400BadRequest);

    public static readonly Error PhotoFileTooLarge =
        new("User.PhotoFileTooLarge", "Photo file is too large. Maximum size is 5MB.", StatusCodes.Status400BadRequest);

    public static readonly Error NoPhotoProvided =
        new("User.NoPhotoProvided", "No photo file was provided.", StatusCodes.Status400BadRequest);

    public static readonly Error DuplicateUsername =
        new("User.DuplicateUsername", "This username is already taken", StatusCodes.Status409Conflict);

    public static readonly Error SameUsername =
        new("User.SameUsername", "New username is the same as the current username", StatusCodes.Status400BadRequest);

    public static readonly Error DuplicatePhoneNumber =
        new("User.DuplicatePhoneNumber", "This phone number is already registered", StatusCodes.Status409Conflict);

    public static readonly Error SamePhoneNumber =
        new("User.SamePhoneNumber", "New phone number is the same as the current phone number", StatusCodes.Status400BadRequest);

    public static readonly Error SameEmail =
        new("User.SameEmail", "New email is the same as the current email", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidPassword =
        new("User.InvalidPassword", "Current password is incorrect", StatusCodes.Status401Unauthorized);

    public static readonly Error SamePassword =
        new("User.SamePassword", "New password must be different from the current password", StatusCodes.Status400BadRequest);
    
    public static readonly Error NoAuthenticate =
        new("User.NoAuthenticate", "Not authenticated request", StatusCodes.Status403Forbidden);
}