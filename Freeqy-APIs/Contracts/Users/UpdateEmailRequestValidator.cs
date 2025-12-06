namespace Freeqy_APIs.Contracts.Users;

public class UpdateEmailRequestValidator : AbstractValidator<UpdateEmailRequest>
{
	public UpdateEmailRequestValidator()
	{
		RuleFor(x => x.NewEmail)
			.NotEmpty().WithMessage("Email is required")
			.EmailAddress().WithMessage("Invalid email format")
			.MaximumLength(256).WithMessage("Email must not exceed 256 characters");

		RuleFor(x => x.CurrentPassword)
			.NotEmpty().WithMessage("Current password is required for security verification");
	}
}
