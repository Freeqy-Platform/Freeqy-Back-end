namespace Freeqy_APIs.Entities;

public class UserEducation
{
	public long Id { get; set; }
	public string UserId { get; set; } = string.Empty;
	public string InstitutionName { get; set; } = string.Empty;
	public string? Degree { get; set; }
	public string? FieldOfStudy { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public string? Grade { get; set; }
	public string? Description { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; set; }

	public ApplicationUser User { get; set; } = default!;
}
