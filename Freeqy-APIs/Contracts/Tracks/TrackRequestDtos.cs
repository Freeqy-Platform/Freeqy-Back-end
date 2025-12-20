namespace Freeqy_APIs.Contracts.Tracks;

public record CreateTrackRequestDto(
	string TrackName
);

public record TrackRequestResponse(
	int Id,
	string TrackName,
	string Status,
	DateTime CreatedAt,
	string? RejectionReason = null,
	string? MergedIntoTrackName = null
);

public record TrackRequestListResponse(
	List<TrackRequestResponse> Requests,
	int TotalCount,
	int PendingCount,
	int ApprovedCount,
	int RejectedCount
);

public record UserTrackRequestStatsResponse(
	int MonthlyRequestsUsed,
	int MonthlyRequestsRemaining,
	int MonthlyLimit,
	bool CanRequestToday,
	DateTime? LastRequestDate,
	DateTime? NextAvailableDate
);

public record ApproveTrackRequestDto(
	int RequestId,
	bool CreateNewTrack = true,
	int? MergeIntoTrackId = null
);

public record RejectTrackRequestDto(
	int RequestId,
	string RejectionReason
);
