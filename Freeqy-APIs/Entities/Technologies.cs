namespace Freeqy_APIs.Entities;

public class Technologies
{
    public string Id { set; get; }
    public string Name { set; get; }
    
    public IEnumerable<Projects> Projects { set; get; } =  new List<Projects>();
}