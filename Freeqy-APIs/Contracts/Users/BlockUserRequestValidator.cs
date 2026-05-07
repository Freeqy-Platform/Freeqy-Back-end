using FluentValidation;

namespace Freeqy_APIs.Contracts.Users;

public class BlockUserRequestValidator : AbstractValidator<BlockUserRequest>
{
	public BlockUserRequestValidator()
	{
		RuleFor(x => x.Reason)
			.NotEmpty().WithMessage("Block reason is required.")
			.MinimumLength(5).WithMessage("Reason must be at least 5 characters.")
			.MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
	}
}
