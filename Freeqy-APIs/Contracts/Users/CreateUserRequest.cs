namespace Freeqy_APIs.Contracts.Users;

public record CreateUserRequest(
	string FirstName,
	string LastName,
	string Email,
	string UserName,
	string Password,
	string? PhoneNumber,
	int? TrackId,
	string? RoleId
);