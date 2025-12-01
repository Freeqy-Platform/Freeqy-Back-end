using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

public class ProjectMembers
{
    public string ProjectId { get; set; }
    public string UserId { get; set; }

    public string Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime JoinedAt { get; set; }

    public Project Project { get; set; }
    public ApplicationUser User { get; set; }
}
