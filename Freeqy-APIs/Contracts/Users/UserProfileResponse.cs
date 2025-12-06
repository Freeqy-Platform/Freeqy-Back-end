namespace Freeqy_APIs.Contracts.Users;

public record UserProfileResponse(
	string Id,
	string Email,
	string UserName,
	string FirstName,
	string LastName,
	string? PhotoUrl,
	string? PhoneNumber,
	string? Summary,
	
	string? Track,
	IEnumerable<SkillResponse>? Skills,
	IEnumerable<SocialMediaLinkDto>? SocialLinks,
	IEnumerable<EducationDto>? Educations,
	IEnumerable<CertificateDto>? Certificates
);