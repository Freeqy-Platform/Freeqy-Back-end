namespace Freeqy_APIs.Contracts.Users;

public class UpdateTrackRequestValidator : AbstractValidator<UpdateTrackRequest>
{
	public UpdateTrackRequestValidator()
	{
		RuleFor(x => x.TrackName)
			.NotEmpty()
			.WithMessage("Track name is required")
			.Length(2, 100)
			.WithMessage("Track name must be between 2 and 100 characters");
	}
}
