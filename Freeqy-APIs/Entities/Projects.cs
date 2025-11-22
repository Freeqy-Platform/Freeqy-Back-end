using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

public class Projects
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Representing status as string (e.g. "InProgress", "Completed")
    public string Status { get; set; }

    // Representing visibility as string (e.g. "Public", "Private")
    public string Visibility { get; set; }

    // Owner of the project
    public string OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public ApplicationUser Owner { get; set; }

    public string EstimatedTime { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Categories Category { get; set; }

    // Navigation properties
    public ICollection<ProjectMembers> ProjectMembers { get; set; } = new List<ProjectMembers>();
    public ICollection<ProjectTechnologies> ProjectTechnologies { get; set; } = new List<ProjectTechnologies>();
}