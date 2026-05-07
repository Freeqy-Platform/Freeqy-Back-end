namespace Freeqy_APIs.Persistancec;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.HasKey(np => new { np.UserId, np.Type });

        builder.Property(np => np.Type)
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.HasOne(np => np.User)
            .WithMany()
            .HasForeignKey(np => np.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
