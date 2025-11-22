using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

public class ProjectTechnologies
{
    public string Id { set; get; }
    
    [ForeignKey("Project")]
    public string ProjectId { set; get; }
    
    public string TechnologyId { set; get; }
    
    public Projects Project { set; get; }
    
    [ForeignKey(nameof(TechnologyId))]
    public Technologies technology { set; get; }
}