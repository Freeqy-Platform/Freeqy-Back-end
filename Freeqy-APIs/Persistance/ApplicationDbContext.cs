namespace Freeqy_APIs.Persistancec;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<ApplicationUser>(options)
{
    // Projects and related
    public DbSet<Projects> Projects { get; set; }
    public DbSet<Categories> Categories { get; set; }
    public DbSet<ProjectMembers> ProjectMembers { get; set; }
    public DbSet<ProjectTechnologies> ProjectTechnologies { get; set; }
    
    // Technologies and Skills
    public DbSet<Technologies> Technologies { get; set; }
    public DbSet<Skills> Skills { get; set; }
    
    // Tags and Tracks
    public DbSet<Tags> Tags { get; set; }
    public DbSet<Tracks> Tracks { get; set; }
    
    // User-related
    public DbSet<UserTags> UserTags { get; set; }
    public DbSet<UserSkills> UserSkills { get; set; }
    public DbSet<UserSocialMedia> UserSocialMedia { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Prevent multiple cascade paths in SQL Server by disabling cascade delete on ProjectMembers -> Project
        builder.Entity<ProjectMembers>()
            .HasOne(pm => pm.Project)
            .WithMany()
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(builder);
    }
}