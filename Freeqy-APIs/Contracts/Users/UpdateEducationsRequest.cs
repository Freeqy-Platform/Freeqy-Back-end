namespace Freeqy_APIs.Contracts.Users;

public record UpdateEducationsRequest(
	IEnumerable<EducationRequest> Educations
);
