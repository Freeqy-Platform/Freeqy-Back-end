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
	public string? PhotoUrl { get; set; }

	public Track? Track { get; set; }
	public IEnumerable<RefreshToken> RefreshTokens { get; set; } = [];
	public ICollection<UserSkill> Skills { set; get; } = [];
	
	public ICollection<ProjectMembers> ProjectMembers { get; set; } = new List<ProjectMembers>();

}