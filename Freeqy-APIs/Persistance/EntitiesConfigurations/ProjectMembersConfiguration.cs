namespace Freeqy_APIs.Persistance.EntitiesConfigurations;

public class ProjectMembersConfiguration: IEntityTypeConfiguration<ProjectMembers>
{
    public void Configure(EntityTypeBuilder<ProjectMembers> builder)
    {
        builder.HasKey(pm => new { pm.ProjectId, pm.UserId });

        builder
            .HasOne(pm => pm.Project)
            .WithMany(p => p.ProjectMembers)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(pm => pm.User)
            .WithMany(u => u.ProjectMembers)
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}