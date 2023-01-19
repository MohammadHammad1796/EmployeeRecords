using EmployeeRecords.Controllers.Apis.Resources;
using FluentValidation;
using System.Reflection;

namespace EmployeeRecords.Controllers.Validators;

public class DataTableQueryResourceValidator<TResource>
    : AbstractValidator<DataTableQueryResource>
    where TResource : class
{
    public DataTableQueryResourceValidator()
    {
        RuleFor(q => q.Sort.By)
            .Custom((sortBy, context) =>
            {
                var property = typeof(TResource)
                    .GetProperty(sortBy,
                        BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (property == null)
                    context
                        .AddFailure(
                            $"{nameof(DataTableQueryResource.Sort)}" +
                            $".{nameof(DataTableQueryResource.Sort.By)}",
                            "Property does not existed");
            });
    }
}