namespace Freeqy_APIs.Entities;

public class Track
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ApplicationUser> Users { get; set; } = [];
}