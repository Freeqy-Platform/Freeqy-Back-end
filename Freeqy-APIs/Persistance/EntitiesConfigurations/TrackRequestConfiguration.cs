namespace Freeqy_APIs.Persistance.EntitiesConfigurations;

public class TrackRequestConfiguration : IEntityTypeConfiguration<TrackRequest>
{
	public void Configure(EntityTypeBuilder<TrackRequest> builder)
	{
		builder.HasKey(tr => tr.Id);
		
		builder.Property(tr => tr.TrackName)
			.IsRequired()
			.HasMaxLength(100);
		
		builder.Property(tr => tr.RequestedBy)
			.IsRequired()
			.HasMaxLength(100);
		
		builder.Property(tr => tr.Status)
			.IsRequired();
		
		builder.Property(tr => tr.RejectionReason)
			.HasMaxLength(500);
		
		builder.HasOne(tr => tr.User)
			.WithMany()
			.HasForeignKey(tr => tr.RequestedBy)
			.OnDelete(DeleteBehavior.Restrict);
		
		builder.HasOne(tr => tr.ApprovedByUser)
			.WithMany()
			.HasForeignKey(tr => tr.ApprovedBy)
			.OnDelete(DeleteBehavior.Restrict);
		
		builder.HasOne(tr => tr.MergedIntoTrack)
			.WithMany()
			.HasForeignKey(tr => tr.MergedIntoTrackId)
			.OnDelete(DeleteBehavior.Restrict);
		
		builder.HasIndex(tr => tr.Status);
		builder.HasIndex(tr => tr.CreatedAt);
		builder.HasIndex(tr => new { tr.TrackName, tr.Status });
	}
}
