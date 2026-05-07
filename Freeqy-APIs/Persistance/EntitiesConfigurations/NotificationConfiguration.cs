namespace Freeqy_APIs.Persistancec;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title).HasMaxLength(256).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(1024).IsRequired();
        builder.Property(n => n.EntityType).HasMaxLength(64);
        builder.Property(n => n.EntityId).HasMaxLength(128);
        builder.Property(n => n.ActionUrl).HasMaxLength(512);

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(n => n.Priority)
            .HasConversion<string>()
            .HasMaxLength(16);

        // Prevent cascade from both Recipient and Actor going to same table
        builder.HasOne(n => n.Recipient)
            .WithMany()
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Actor)
            .WithMany()
            .HasForeignKey(n => n.ActorId)
            .OnDelete(DeleteBehavior.NoAction);

        // Primary query pattern: "get user's unread notifications, newest first"
        builder.HasIndex(n => new { n.RecipientId, n.IsRead, n.CreatedAt })
            .HasDatabaseName("IX_Notifications_Recipient_Read_Created")
            .IsDescending(false, false, true);

        // Cleanup job: find old notifications
        builder.HasIndex(n => n.CreatedAt)
            .HasDatabaseName("IX_Notifications_CreatedAt");

        // Deduplication: prevent duplicate notifications per recipient for same entity
        builder.HasIndex(n => new { n.RecipientId, n.Type, n.EntityId })
            .HasDatabaseName("IX_Notifications_Dedup");
    }
}
