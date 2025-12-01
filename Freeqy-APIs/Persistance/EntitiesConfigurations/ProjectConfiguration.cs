namespace Freeqy_APIs.Persistance.EntitiesConfigurations;

public class ProjectConfiguration: IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.Property(p => p.Id)
            .HasMaxLength(100);
    }
}