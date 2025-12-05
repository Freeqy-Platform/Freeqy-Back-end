namespace Freeqy_APIs.Contracts.Users;

public class UpdateUsernameRequestValidator : AbstractValidator<UpdateUsernameRequest>
{
	public UpdateUsernameRequestValidator()
	{
		RuleFor(x => x.NewUsername)
			.NotEmpty().WithMessage("Username is required")
			.MinimumLength(3).WithMessage("Username must be at least 3 characters")
			.MaximumLength(50).WithMessage("Username must not exceed 50 characters")
			.Matches("^[a-zA-Z0-9_.-]+$")
			.WithMessage("Username can only contain letters, numbers, dots, hyphens, and underscores");
	}
}
