using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

public class Project
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Description { get; set; }

    // Representing status as string (e.g. "InProgress", "Completed")
    public ProjectStatus Status { get; set; } = ProjectStatus.Pending;

    // Representing visibility as string (e.g. "Public", "Private")
    public ProjectVisibility Visibility { get; set; } =  ProjectVisibility.Public;

    // Owner of the project
    public string OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public ApplicationUser Owner { get; set; }

    public TimeSpan EstimatedTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }

    public string CategoryId { get; set; }
    
    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; }
    
    // public ProjectCategory Category { get; set; }

    // Navigation properties
    public ICollection<ProjectMembers> ProjectMembers { get; set; } =[];
    public List<Technology> Technologies { get; set; } = new();
    public DateTime? DeletedAt { get; set; } = null;
    public bool IsDeleted => DeletedAt.HasValue;
}