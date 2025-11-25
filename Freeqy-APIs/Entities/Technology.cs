namespace Freeqy_APIs.Entities;

public class Technology
{
    public string Id { set; get; }
    public string Name { set; get; }
    
    public IEnumerable<Project> Projects { set; get; } =  new List<Project>();
}