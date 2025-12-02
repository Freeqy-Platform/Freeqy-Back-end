namespace Freeqy_APIs.Contracts.Projects;

public class SendProjectInvitationValidator : AbstractValidator<SendProjectInvitationRequest>
{
    public SendProjectInvitationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format");
    }
}

public class RespondToInvitationValidator : AbstractValidator<RespondToInvitationRequest>
{
    public RespondToInvitationValidator()
    {
        RuleFor(x => x.InvitationId)
            .NotEmpty()
            .WithMessage("Invitation ID is required");

        RuleFor(x => x.Accept)
            .NotNull();

        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .When(x => !x.Accept)
            .WithMessage("Rejection reason is required when rejecting");
    }
}
