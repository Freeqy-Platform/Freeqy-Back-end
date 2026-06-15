using Freeqy_APIs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freeqy_APIs.Persistance.EntitiesConfigurations;

public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasMaxLength(100);

        builder.Property(m => m.ProjectId)
            .IsRequired();

        builder.Property(m => m.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasMaxLength(1000);

        builder.Property(m => m.MeetingLink)
            .HasMaxLength(500);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.HasOne(m => m.Project)
            .WithMany()
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.ProjectId);
        builder.HasIndex(m => new { m.ProjectId, m.ScheduledAt });
    }
}
