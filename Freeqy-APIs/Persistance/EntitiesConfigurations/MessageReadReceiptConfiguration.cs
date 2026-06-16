namespace Freeqy_APIs.Persistancec;

public class MessageReadReceiptConfiguration : IEntityTypeConfiguration<MessageReadReceipt>
{
    public void Configure(EntityTypeBuilder<MessageReadReceipt> builder)
    {
        builder.HasKey(rr => new { rr.MessageId, rr.UserId });

        builder.HasOne(rr => rr.Message)
            .WithMany(m => m.ReadReceipts)
            .HasForeignKey(rr => rr.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rr => rr.User)
            .WithMany()
            .HasForeignKey(rr => rr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(rr => rr.MessageId);

        builder.HasIndex(rr => new { rr.UserId, rr.ReadAt });
    }
}
