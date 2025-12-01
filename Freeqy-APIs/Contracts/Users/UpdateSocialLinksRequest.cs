namespace Freeqy_APIs.Contracts.Users;

public record UpdateSocialLinksRequest(
	IEnumerable<SocialMediaLinkDto> SocialLinks
);
