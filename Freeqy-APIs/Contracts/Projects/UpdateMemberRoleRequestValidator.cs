using FluentValidation;

namespace Freeqy_APIs.Contracts.Projects;

public class UpdateMemberRoleRequestValidator : AbstractValidator<UpdateMemberRoleRequest>
{
    public UpdateMemberRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .MaximumLength(50).WithMessage("Role cannot exceed 50 characters")
            .Must(BeAValidRole).WithMessage("Role must be one of: Member, Admin, Lead, Developer, Designer");
    }

    private bool BeAValidRole(string role)
    {
        var validRoles = new[] { "Member", "Admin", "Lead", "Developer", "Designer", "Tester", "Manager" };
        return validRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
