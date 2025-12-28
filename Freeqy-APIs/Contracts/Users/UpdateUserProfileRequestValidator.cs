namespace Freeqy_APIs.Contracts.Users;

public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
	public UpdateUserProfileRequestValidator()
	{
		When(x => !string.IsNullOrWhiteSpace(x.FirstName), () =>
		{
			RuleFor(x => x.FirstName)
				.Length(3, 100)
				.WithMessage("First name must be between 3 and 100 characters");
		});

		When(x => !string.IsNullOrWhiteSpace(x.LastName), () =>
		{
			RuleFor(x => x.LastName)
				.Length(3, 100)
				.WithMessage("Last name must be between 3 and 100 characters");
		});

		When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
		{
			RuleFor(x => x.PhoneNumber)
				.Matches(@"^\+?[1-9]\d{1,14}$")
				.WithMessage("Phone number must be in valid international format");
		});

		When(x => !string.IsNullOrWhiteSpace(x.Summary), () =>
		{
			RuleFor(x => x.Summary)
				.MaximumLength(1000)
				.WithMessage("Summary must not exceed 1000 characters");
		});

		When(x => !string.IsNullOrWhiteSpace(x.Availability), () =>
		{
			RuleFor(x => x.Availability)
				.Must(a => Enum.TryParse<UserAvailability>(a, true, out _))
				.WithMessage("Availability must be one of: Available, Busy, NotAvailable");
		});

		When(x => !string.IsNullOrWhiteSpace(x.TrackName), () =>
		{
			RuleFor(x => x.TrackName)
				.Length(2, 100)
				.WithMessage("Track name must be between 2 and 100 characters");
		});
	}
}