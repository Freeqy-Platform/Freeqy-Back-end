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
	}
}