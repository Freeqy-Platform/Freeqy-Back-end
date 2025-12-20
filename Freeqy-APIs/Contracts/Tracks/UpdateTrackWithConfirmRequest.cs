namespace Freeqy_APIs.Contracts.Tracks;

public record UpdateTrackWithConfirmRequest(
	string TrackName,
	bool ConfirmCreate = false
);
