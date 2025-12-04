namespace Freeqy_APIs.Contracts.Users;

public record EducationRequest(
	string InstitutionName,
	string? Degree,
	string? FieldOfStudy,
	DateTime? StartDate,
	DateTime? EndDate,
	string? Grade,
	string? Description
);
