using Freeqy_APIs.Abstractions;

namespace Freeqy_APIs.Errors;

public static class PasswordResetErrors
{
    public static readonly Error UserNotFound = new(
        "PasswordReset.UserNotFound",
        "No user found with the provided email address.",
        404
    );

    public static readonly Error InvalidToken = new(
        "PasswordReset.InvalidToken",
        "The password reset token is invalid.",
        400
    );

    public static readonly Error ExpiredToken = new(
        "PasswordReset.ExpiredToken",
        "The password reset token has expired. Please request a new one.",
        400
    );

    public static readonly Error TokenAlreadyUsed = new(
        "PasswordReset.TokenAlreadyUsed",
        "This password reset token has already been used.",
        400
    );

    public static readonly Error EmailSendFailed = new(
        "PasswordReset.EmailSendFailed",
        "Failed to send password reset email. Please try again later.",
        500
    );

    public static readonly Error InvalidPassword = new(
        "PasswordReset.InvalidPassword",
        "The provided password does not meet security requirements.",
        400
    );
}
