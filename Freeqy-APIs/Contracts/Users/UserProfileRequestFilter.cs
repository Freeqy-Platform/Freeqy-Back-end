namespace Freeqy_APIs.Contracts.Users;

public record UserProfileRequestFilter(
	string? Name,
	string? Track,
	List<string>? Skills
);