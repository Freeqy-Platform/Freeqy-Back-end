namespace Freeqy_APIs.Contracts.Tracks;

public class UpdateTrackWithConfirmRequestValidator : AbstractValidator<UpdateTrackWithConfirmRequest>
{
	public UpdateTrackWithConfirmRequestValidator()
	{
		RuleFor(x => x.TrackName)
			.NotEmpty()
			.WithMessage("Track name is required")
			.Length(2, 100)
			.WithMessage("Track name must be between 2 and 100 characters");
	}
}
