namespace Freeqy_APIs.Contracts.Users;

public class UpdateAvailabilityRequestValidator : AbstractValidator<UpdateAvailabilityRequest>
{
	public UpdateAvailabilityRequestValidator()
	{
		RuleFor(x => x.Availability)
			.NotEmpty().WithMessage("Availability status is required")
			.Must(BeValidAvailability).WithMessage("Availability must be one of: Available, Busy, DoNotDisturb");
	}

	private bool BeValidAvailability(string availability)
	{
		return availability is "Available" or "Busy" or "DoNotDisturb";
	}
}
