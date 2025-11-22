using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

public class UserSkills
{
    public string Id { get; set; }

    public string UserId { get; set; }
    public string SkillId { get; set; }

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    [ForeignKey(nameof(SkillId))]
    public Skills Skill { get; set; }
}