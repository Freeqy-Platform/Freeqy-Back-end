using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

public class Projects
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Status { get; set; }
    public bool Visible { get; set; }
    
    
    public string CategoryId { get; set; }
    
    [ForeignKey(nameof(CategoryId))]
    public Categories Category { get; set; }
}