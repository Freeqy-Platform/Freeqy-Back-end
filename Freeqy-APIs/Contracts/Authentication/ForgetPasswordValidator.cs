

namespace Freeqy_APIs.Contracts.Authentication;

public class ForgetPasswordValidator: AbstractValidator<ForgetPasswordRequest>
{

    public ForgetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email address");

    }
    
}