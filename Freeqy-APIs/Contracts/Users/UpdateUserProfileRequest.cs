namespace Freeqy_APIs.Contracts.Users;

public record UpdateUserProfileRequest(
	string? FirstName,
	string? LastName,
	string? PhoneNumber,
	string? Summary,
	string? Availability,
	string? TrackName
);
