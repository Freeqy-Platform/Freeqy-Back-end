namespace Freeqy_APIs.Contracts.Users;

public class UpdateAvailabilityRequestValidator : AbstractValidator<UpdateAvailabilityRequest>
{
	public UpdateAvailabilityRequestValidator()
	{
		RuleFor(x => x.Availability)
			.IsInEnum().WithMessage("Availability must be 1 (Available), 2 (Busy), or 3 (DoNotDisturb)");
	}
}
