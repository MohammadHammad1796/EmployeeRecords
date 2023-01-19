using EmployeeRecords.Controllers.Apis.Resources;
using FluentValidation;

namespace EmployeeRecords.Controllers.Validators;

public class SaveEmployeeResourceValidator : AbstractValidator<SaveEmployeeResource>
{
    public SaveEmployeeResourceValidator()
    {
        RuleFor(e => e.DepartmentId)
            .NotEmpty()
            .WithMessage("DepartmentId is required");

        RuleFor(e => e.DateOfBirth)
            .NotEmpty()
            .WithMessage("Date of birth is required")
            .Must(d => DateTime.UtcNow.Year - d.Year >= 18)
            .WithMessage("Date of birth should be at least since 18 years");
    }
}