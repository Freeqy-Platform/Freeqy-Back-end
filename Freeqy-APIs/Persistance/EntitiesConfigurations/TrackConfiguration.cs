namespace Freeqy_APIs.Persistance.EntitiesConfigurations;

public class TrackConfiguration : IEntityTypeConfiguration<Track>
{
	public void Configure(EntityTypeBuilder<Track> builder)
	{
		builder.Property(x => x.Name).HasMaxLength(50);
	}
}