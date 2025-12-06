namespace Freeqy_APIs.Contracts.Users;

public record UpdatePasswordRequest(
	string CurrentPassword,
	string NewPassword,
	string ConfirmNewPassword
);
