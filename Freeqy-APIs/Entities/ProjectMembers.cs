using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

public class ProjectMembers
{
    public string Id { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime JoinedAt { get; set; }
    

    public string ProjectId { get; set; }
    public string UserId { get; set; }
    
    [ForeignKey("ProjectId")]
    public Projects Project { get; set; }
    
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }
    
}