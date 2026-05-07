namespace Freeqy_APIs.Entities;

public class Project
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Description { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Pending;

    public ProjectVisibility Visibility { get; set; } =  ProjectVisibility.Public;

    // Owner of the project
    public string OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }

    public string CategoryId { get; set; }
    public Category? Category { get; set; }
    
    // public ProjectCategory Category { get; set; }

    public ICollection<ProjectMembers> ProjectMembers { get; set; } =[];
    public List<Technology> Technologies { get; set; } = [];
    public DateTime? DeletedAt { get; set; } = null;
    public bool IsDeleted => DeletedAt.HasValue;
}