namespace Freeqy_APIs.Persistance.EntitiesConfigurations;

public class ProjectInvitationConfiguration : IEntityTypeConfiguration<ProjectInvitation>
{
    public void Configure(EntityTypeBuilder<ProjectInvitation> builder)
    {
        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.Id)
            .HasMaxLength(100);

        builder.Property(pi => pi.ProjectId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pi => pi.InvitedEmail)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(pi => pi.SentByUserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pi => pi.InvitedUserId)
            .HasMaxLength(100);

        builder.Property(pi => pi.RespondedReason)
            .HasMaxLength(500);

        builder
            .HasOne(pi => pi.Project)
            .WithMany()
            .HasForeignKey(pi => pi.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(pi => pi.SentByUser)
            .WithMany()
            .HasForeignKey(pi => pi.SentByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(pi => pi.InvitedUser)
            .WithMany()
            .HasForeignKey(pi => pi.InvitedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pi => new { pi.ProjectId, pi.InvitedEmail });
    }
}
