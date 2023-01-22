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
        RuleFor(q => q.Sort)
            .Custom((sort, context) =>
            {
                if (sort == null)
                    return;

                var property = typeof(TResource)
                    .GetProperty(sort.By,
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