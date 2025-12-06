namespace Freeqy_APIs.Contracts.Users;

public record UpdateEmailRequest(
	string NewEmail,
	string CurrentPassword
);
