namespace Freeqy_APIs.Persistancec;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.Title)
            .HasMaxLength(200);

        builder.Property(c => c.ChannelName)
            .HasMaxLength(100);

        builder.Property(c => c.CreatedByUserId)
            .IsRequired();

        builder.HasOne(c => c.Project)
            .WithMany()
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.Type, c.LastMessageAt })
            .IsDescending(false, true);

        builder.HasIndex(c => c.ProjectId);

        // Unique constraint: one channel name per project
        builder.HasIndex(c => new { c.ProjectId, c.ChannelName })
            .IsUnique()
            .HasFilter("[ProjectId] IS NOT NULL AND [ChannelName] IS NOT NULL");
    }
}
