namespace Freeqy_APIs.Persistance.EntitiesConfigurations;

public class SkillsConfiguration : IEntityTypeConfiguration<Skill>
{
	public void Configure(EntityTypeBuilder<Skill> builder)
	{
		builder.Property(x => x.Name).HasMaxLength(50);
	}
}