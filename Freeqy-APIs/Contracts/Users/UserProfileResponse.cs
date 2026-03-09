using Freeqy_APIs.Contracts.Badges;

namespace Freeqy_APIs.Contracts.Users;

public record UserProfileResponse(
	string Id,
	string Email,
	string UserName,
	string FirstName,
	string LastName,
	string? PhotoUrl,
	string? BannerPhotoUrl,
	string? PhoneNumber,
	string? Summary,
	string Availability,

	string? Track,
	IEnumerable<SkillResponse>? Skills,
	IEnumerable<SocialMediaLinkDto>? SocialLinks,
	IEnumerable<EducationDto>? Educations,
	IEnumerable<CertificateDto>? Certificates,
	IEnumerable<BadgeResponse>? Badges = null
);
