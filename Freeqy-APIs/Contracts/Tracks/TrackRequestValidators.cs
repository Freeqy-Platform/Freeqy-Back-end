namespace Freeqy_APIs.Contracts.Tracks;

public class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequestDto>
{
	public CreateTrackRequestValidator()
	{
		RuleFor(x => x.TrackName)
			.NotEmpty()
			.WithMessage("Track name is required")
			.Length(2, 100)
			.WithMessage("Track name must be between 2 and 100 characters");
	}
}

public class ApproveTrackRequestValidator : AbstractValidator<ApproveTrackRequestDto>
{
	public ApproveTrackRequestValidator()
	{
		RuleFor(x => x.RequestId)
			.GreaterThan(0)
			.WithMessage("Request ID must be greater than 0");
		
		When(x => !x.CreateNewTrack, () =>
		{
			RuleFor(x => x.MergeIntoTrackId)
				.NotNull()
				.WithMessage("Merge into track ID is required when not creating a new track")
				.GreaterThan(0)
				.WithMessage("Merge into track ID must be greater than 0");
		});
	}
}

public class RejectTrackRequestValidator : AbstractValidator<RejectTrackRequestDto>
{
	public RejectTrackRequestValidator()
	{
		RuleFor(x => x.RequestId)
			.GreaterThan(0)
			.WithMessage("Request ID must be greater than 0");
		
		RuleFor(x => x.RejectionReason)
			.NotEmpty()
			.WithMessage("Rejection reason is required")
			.MaximumLength(500)
			.WithMessage("Rejection reason must not exceed 500 characters");
	}
}
