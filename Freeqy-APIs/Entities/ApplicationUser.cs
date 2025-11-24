namespace Freeqy_APIs.Entities;

public sealed class ApplicationUser : IdentityUser
{
	public ApplicationUser()
	{
		Id = Guid.CreateVersion7().ToString();
		SecurityStamp = Guid.CreateVersion7().ToString();
	}

	public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
	public int? TrackId { get; set; }

	public Track? Track { get; set; }
	public IEnumerable<RefreshToken> RefreshTokens { get; set; } = [];
	public IEnumerable<UserSkill> Skills { set; get; } = [];
}