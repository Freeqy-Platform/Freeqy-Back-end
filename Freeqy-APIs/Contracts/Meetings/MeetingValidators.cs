using FluentValidation;

namespace Freeqy_APIs.Contracts.Meetings;

public class CreateMeetingRequestValidator : AbstractValidator<CreateMeetingRequest>
{
    public CreateMeetingRequestValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.ScheduledAt)
            .NotEmpty().WithMessage("Scheduled date and time is required")
            .GreaterThan(DateTime.UtcNow).WithMessage("Meeting must be scheduled for a future date");

        RuleFor(x => x.MeetingLink)
            .MaximumLength(500).WithMessage("Meeting link must not exceed 500 characters");
    }
}

public class UpdateMeetingRequestValidator : AbstractValidator<UpdateMeetingRequest>
{
    public UpdateMeetingRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.ScheduledAt)
            .NotEmpty().WithMessage("Scheduled date and time is required")
            .GreaterThan(DateTime.UtcNow).WithMessage("Meeting must be scheduled for a future date");

        RuleFor(x => x.MeetingLink)
            .MaximumLength(500).WithMessage("Meeting link must not exceed 500 characters");
    }
}
