using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeRecords.Custom.Extensions;

public static class ControllerExtensions
{
    public static StatusCodeResult ServerError(this Controller _)
        => new(StatusCodes.Status500InternalServerError);

    public static bool ValidateWithFluent<T>(this Controller controller,
        AbstractValidator<T> validator, T resource)
    {
        var validationResult = validator.Validate(resource);
        if (validationResult.Errors.Any())
            validationResult.Errors
                .ForEach(failure => controller.ModelState
                    .AddModelError(failure.PropertyName, failure.ErrorMessage));

        return controller.ModelState.IsValid;
    }
}