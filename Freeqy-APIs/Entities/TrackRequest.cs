using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

public class TrackRequest
{
	public int Id { get; set; }
	
	public string RequestedBy { get; set; } = string.Empty;
	
	public ApplicationUser? User { get; set; }
	
	public string TrackName { get; set; } = string.Empty;
	
	public TrackRequestStatus Status { get; set; } = TrackRequestStatus.Pending;
	
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	
	public string? RejectionReason { get; set; }
	
	public string? ApprovedBy { get; set; }
	
	public ApplicationUser? ApprovedByUser { get; set; }
	
	public DateTime? ApprovedAt { get; set; }
	
	public int? MergedIntoTrackId { get; set; }
	
	public Track? MergedIntoTrack { get; set; }
}
