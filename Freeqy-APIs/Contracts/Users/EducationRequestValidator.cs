namespace Freeqy_APIs.Contracts.Users;

public class EducationRequestValidator : AbstractValidator<EducationRequest>
{
	public EducationRequestValidator()
	{
		RuleFor(x => x.InstitutionName)
			.NotEmpty().WithMessage("Institution name is required")
			.MaximumLength(200).WithMessage("Institution name must not exceed 200 characters");

		RuleFor(x => x.Degree)
			.MaximumLength(200).WithMessage("Degree must not exceed 200 characters")
			.When(x => !string.IsNullOrEmpty(x.Degree));

		RuleFor(x => x.FieldOfStudy)
			.MaximumLength(200).WithMessage("Field of study must not exceed 200 characters")
			.When(x => !string.IsNullOrEmpty(x.FieldOfStudy));

		RuleFor(x => x.Grade)
			.MaximumLength(50).WithMessage("Grade must not exceed 50 characters")
			.When(x => !string.IsNullOrEmpty(x.Grade));

		RuleFor(x => x.EndDate)
			.GreaterThanOrEqualTo(x => x.StartDate)
			.WithMessage("End date must be after or equal to start date")
			.When(x => x.StartDate.HasValue && x.EndDate.HasValue);
	}
}
