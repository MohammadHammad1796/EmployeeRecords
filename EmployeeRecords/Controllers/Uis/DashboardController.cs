using EmployeeRecords.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeRecords.Controllers.Uis;

[Route("")]
public class DashboardController : Controller
{
    private readonly IEmployeesRepository _employeesRepository;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IEmployeesRepository employeesRepository,
        ILogger<DashboardController> logger)
    {
        _employeesRepository = employeesRepository;
        _logger = logger;
    }

    [Route("")]
    public IActionResult Home() => View("Home");

    [Route("departments")]
    public IActionResult Departments() => View("Departments");

    [Route("employees")]
    public IActionResult Employees() => View("Employees");

    [Route("employees/{id:int}/files")]
    public async Task<IActionResult> EmployeeFiles([FromRoute] int id)
    {
        try
        {
            var employeeExisted = await _employeesRepository.IsRecordExistedAsync(id);
            if (!employeeExisted)
                return NotFound();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occur while trying to check employee if existed");
            return View("Error");
        }

        ViewData["employeeId"] = id;
        return View("EmployeeFiles");
    }

    [Route("notfound")]
    public new ViewResult NotFound()
    {
        if (Response.StatusCode != StatusCodes.Status404NotFound)
            Response.HttpContext.Items["statusSetManually"] = true;
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("NotFound");
    }
}