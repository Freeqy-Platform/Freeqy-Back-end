namespace Freeqy_APIs.Contracts.Users;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
	public CreateUserRequestValidator()
	{
		RuleFor(x => x.FirstName)
			.NotEmpty().WithMessage("First name is required.")
			.MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

		RuleFor(x => x.LastName)
			.NotEmpty().WithMessage("Last name is required.")
			.MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email is required.")
			.EmailAddress().WithMessage("A valid email address is required.");

		RuleFor(x => x.UserName)
			.NotEmpty().WithMessage("Username is required.")
			.MinimumLength(3).WithMessage("Username must be at least 3 characters.")
			.MaximumLength(30).WithMessage("Username must not exceed 30 characters.")
			.Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Username may only contain letters, numbers, and underscores.");

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required.")
			.MinimumLength(8).WithMessage("Password must be at least 8 characters.");

		RuleFor(x => x.PhoneNumber)
			.Matches(@"^\+?[0-9\s\-().]{7,20}$").WithMessage("Phone number format is invalid.")
			.When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
	}
}