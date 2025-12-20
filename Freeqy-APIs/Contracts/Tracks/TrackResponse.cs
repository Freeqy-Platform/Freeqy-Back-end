namespace Freeqy_APIs.Contracts.Tracks;

public record TrackResponse(
	int Id,
	string Name
);

public record TrackSuggestionResponse(
	string? Message,
	List<string>? SuggestedTracks
);
