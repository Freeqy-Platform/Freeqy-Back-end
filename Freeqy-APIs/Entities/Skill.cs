namespace Freeqy_APIs.Entities;

public class Skill
{
    public int Id { set; get; }
    public string Name { set; get; } = string.Empty;

    public ICollection<UserSkill> Users { set; get; } = [];
}