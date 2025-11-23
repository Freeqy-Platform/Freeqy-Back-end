namespace Freeqy_APIs.Entities;

public class UserSkill
{
	public string UserId { get; set; } = string.Empty;
	public int SkillId { get; set; }

	public ApplicationUser? User { get; set; }
	public Skill? Skill { get; set; }
}