namespace Freeqy_APIs.Contracts.Users;

public class UpdatePhoneNumberRequestValidator : AbstractValidator<UpdatePhoneNumberRequest>
{
	public UpdatePhoneNumberRequestValidator()
	{
		RuleFor(x => x.PhoneNumber)
			.NotEmpty().WithMessage("Phone number is required")
			.Matches(@"^\+?[1-9]\d{1,14}$")
			.WithMessage("Phone number must be a valid international format (E.164). Example: +201234567890")
			.MinimumLength(10).WithMessage("Phone number must be at least 10 digits")
			.MaximumLength(15).WithMessage("Phone number must not exceed 15 digits");
	}
}
