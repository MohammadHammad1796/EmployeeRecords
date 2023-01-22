using EmployeeRecords.Controllers.Apis.Resources;
using EmployeeRecords.Core.Helpers;
using FluentValidation;

namespace EmployeeRecords.Controllers.Validators;

public class UpdateEmployeeFileValidator : AbstractValidator<UpdateEmployeeFileResource>
{
    public UpdateEmployeeFileValidator(FileCriteria criteria)
    {
        RuleFor(r => r.File)
            .Must(f => f?.Length <= 1024 * 1024 * criteria.MaxSizeInMb)
            .When(r => r.File != null)
            .WithMessage("File maximum allowed size is 5 Mb");

        RuleFor(r => r.File)
            .Must(f => criteria.AllowedContentTypes.Any(a =>
                a.Equals(f?.ContentType, StringComparison.CurrentCultureIgnoreCase)))
            .When(r => r.File != null)
            .WithMessage("File type is not allowed");
    }
}