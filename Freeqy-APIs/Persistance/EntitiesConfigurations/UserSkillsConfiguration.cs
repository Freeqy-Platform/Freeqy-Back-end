namespace Freeqy_APIs.Persistance.EntitiesConfigurations;

public class UserSkillsConfiguration : IEntityTypeConfiguration<UserSkill>
{
	public void Configure(EntityTypeBuilder<UserSkill> builder)
	{
		builder.HasKey(e => new { e.UserId, e.SkillId });
	}
}