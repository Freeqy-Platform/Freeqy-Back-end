namespace Freeqy_APIs.Contracts.Users;

public record UserProfileResponse(
	string Id,
	string Email,
	string UserName,
	string FirstName,
	string LastName,
	string? PhotoUrl
	string LastName,
	string? Track,
	IEnumerable<SkillResponse>? Skills
);