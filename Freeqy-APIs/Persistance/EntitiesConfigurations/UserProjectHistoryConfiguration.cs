namespace Freeqy_APIs.Persistance.EntitiesConfigurations;

public class UserProjectHistoryConfiguration : IEntityTypeConfiguration<UserProjectHistory>
{
    public void Configure(EntityTypeBuilder<UserProjectHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ProjectCategory)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasMaxLength(50);

        builder.Property(x => x.EventType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.ProjectStatusAtEvent)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => new { x.UserId, x.EventDate })
            .IsDescending(false, true);
    }
}
