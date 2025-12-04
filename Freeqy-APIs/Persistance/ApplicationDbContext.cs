namespace Freeqy_APIs.Persistancec;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProjectMembers> ProjectMembers { get; set; }
    public DbSet<ProjectInvitation> ProjectInvitations { get; set; }
    public DbSet<Technology> Technologies { get; set; }
    public DbSet<Tags> Tags { get; set; }
    public DbSet<UserTags> UserTags { get; set; }
    public DbSet<UserSocialMedia> UserSocialMedia { get; set; }
    public DbSet<UserEducation> UserEducations { get; set; }
    public DbSet<UserCertificate> UserCertificates { get; set; }
    public DbSet<Skill> Skills { get; set; }    
    public DbSet<UserSkill> UserSkills{ get; set; }    
    public DbSet<Track> Tracks{ get; set; }    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


        /* Prevent multiple cascade paths in SQL Server by disabling cascade delete on ProjectMembers -> Project
        builder.Entity<ProjectMembers>()
            .HasOne(pm => pm.Project)
            .WithMany()
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
            */

        base.OnModelCreating(builder);
    }
}