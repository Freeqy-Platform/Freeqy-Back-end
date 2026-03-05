namespace Freeqy_APIs.Contracts.Messaging;

public class StartDirectConversationRequestValidator : AbstractValidator<StartDirectConversationRequest>
{
    public StartDirectConversationRequestValidator()
    {
        RuleFor(x => x.RecipientUserId)
            .NotEmpty().WithMessage("RecipientUserId is required.");
    }
}

public class StartTeamConversationRequestValidator : AbstractValidator<StartTeamConversationRequest>
{
    public StartTeamConversationRequestValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .When(x => x.Title is not null);
    }
}

public class CreateChannelRequestValidator : AbstractValidator<CreateChannelRequest>
{
    public CreateChannelRequestValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.ChannelName)
            .NotEmpty().WithMessage("ChannelName is required.")
            .MaximumLength(100).WithMessage("ChannelName must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z0-9\s\-_]+$").WithMessage("ChannelName can only contain letters, numbers, spaces, hyphens, and underscores.");

        RuleFor(x => x.MemberUserIds)
            .Must(ids => ids == null || ids.Count > 0)
            .WithMessage("If provided, MemberUserIds must contain at least one user.");
    }
}

public class UpdateChannelRequestValidator : AbstractValidator<UpdateChannelRequest>
{
    public UpdateChannelRequestValidator()
    {
        RuleFor(x => x.ChannelName)
            .MaximumLength(100).WithMessage("ChannelName must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z0-9\s\-_]+$").WithMessage("ChannelName can only contain letters, numbers, spaces, hyphens, and underscores.")
            .When(x => x.ChannelName is not null);
    }
}

public class AddChannelMembersRequestValidator : AbstractValidator<AddChannelMembersRequest>
{
    public AddChannelMembersRequestValidator()
    {
        RuleFor(x => x.UserIds)
            .NotEmpty().WithMessage("UserIds is required.")
            .Must(ids => ids.Count <= 50).WithMessage("Cannot add more than 50 members at once.");
    }
}

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required.")
            .MaximumLength(4000).WithMessage("Message must not exceed 4000 characters.");
    }
}

public class EditMessageRequestValidator : AbstractValidator<EditMessageRequest>
{
    public EditMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required.")
            .MaximumLength(4000).WithMessage("Message must not exceed 4000 characters.");
    }
}
