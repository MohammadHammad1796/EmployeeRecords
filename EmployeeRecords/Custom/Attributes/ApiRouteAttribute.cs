using Microsoft.AspNetCore.Mvc;

namespace EmployeeRecords.Custom.Attributes;

public class ApiRouteAttribute : RouteAttribute
{
    public ApiRouteAttribute(string template)
        : base($"api/{template}")
    {
    }
}