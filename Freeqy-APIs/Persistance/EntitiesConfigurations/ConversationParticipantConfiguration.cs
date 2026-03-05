namespace Freeqy_APIs.Persistancec;

public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.HasKey(cp => new { cp.ConversationId, cp.UserId });

        builder.Property(cp => cp.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(cp => cp.Conversation)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.User)
            .WithMany()
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cp => cp.UserId);
    }
}
