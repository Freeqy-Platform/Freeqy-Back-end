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
    public DbSet<TrackRequest> TrackRequests { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Composite PK on UserBadge prevents a user earning the same badge twice
        builder.Entity<UserBadge>()
            .HasKey(ub => new { ub.UserId, ub.BadgeId });

        // Seed the six badge definitions (Id values match BadgeType enum)
        builder.Entity<Badge>().HasData(
            new Badge { Id = 1, Type = BadgeType.FirstProject,     Name = "First Project",     Description = "Created your first project"            },
            new Badge { Id = 2, Type = BadgeType.TeamPlayer,       Name = "Team Player",        Description = "Joined 5 projects as a member"         },
            new Badge { Id = 3, Type = BadgeType.ProjectCompleter, Name = "Project Completer",  Description = "Participated in 3 completed projects"  },
            new Badge { Id = 4, Type = BadgeType.EarlyAdopter,     Name = "Early Adopter",      Description = "One of the first 100 users on Freeqy"  },
            new Badge { Id = 5, Type = BadgeType.Mentor,           Name = "Mentor",             Description = "Owned and led 5 projects"              },
            new Badge { Id = 6, Type = BadgeType.TopContributor,   Name = "Top Contributor",    Description = "Achieved a contribution score over 500"}
        );

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