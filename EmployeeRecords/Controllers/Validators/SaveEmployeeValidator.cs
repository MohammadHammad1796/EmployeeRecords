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

        var thisYear = DateTime.UtcNow.Year;
        RuleFor(e => e.DateOfBirth)
            .NotEmpty()
            .WithMessage("Date of birth is required")
            .Must(d => thisYear - d.Year >= 18)
            .WithMessage("Date of birth should be at least since 18 years")
            .Must(d => thisYear - d.Year <= 60)
            .WithMessage("Date of birth should be at maximum since 60 years");
    }
}