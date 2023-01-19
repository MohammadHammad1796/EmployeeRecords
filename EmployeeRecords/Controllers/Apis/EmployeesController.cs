using AutoMapper;
using EmployeeRecords.Controllers.Apis.Resources;
using EmployeeRecords.Controllers.Validators;
using EmployeeRecords.Core.Models;
using EmployeeRecords.Core.Repositories;
using EmployeeRecords.Custom.Attributes;
using EmployeeRecords.Custom.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeRecords.Controllers.Apis;

[ApiRoute("employees")]
public class EmployeesController : Controller
{
    private readonly IEmployeesRepository _employeesRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IEmployeesFilesRepository _employeesFilesRepository;
    private readonly IFilesRepository _filesRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(
        IMapper mapper,
        IEmployeesRepository employeesRepository,
        IDepartmentsRepository departmentsRepository,
        IEmployeesFilesRepository employeesFilesRepository,
        IFilesRepository filesRepository,
        ILogger<EmployeesController> logger)
    {
        _mapper = mapper;
        _departmentsRepository = departmentsRepository;
        _employeesFilesRepository = employeesFilesRepository;
        _filesRepository = filesRepository;
        _logger = logger;
        _employeesRepository = employeesRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] DataTableQueryResource resource)
    {
        if (!this.ValidateWithFluent(new DataTableQueryResourceValidator<EmployeeResource>(), resource))
            return BadRequest(ModelState);

        var query = resource.ToQuery();
        if (!string.IsNullOrWhiteSpace(resource.SearchQuery))
            query.SearchQuery = resource.SearchQuery;

        var employees = await _employeesRepository.GetAsync(query);
        if (!employees.Any())
            return Ok(employees);

        var employeeResources = _mapper.Map<IEnumerable<EmployeeResource>>(employees);
        if (!resource.WithTotal)
            return Ok(employeeResources);

        var total = await _employeesRepository.GetCountAsync(query.SearchQuery);
        return Ok(new ItemsWithTotalResource<EmployeeResource>(total, employeeResources));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
    {
        var employee = await _employeesRepository.GetByIdAsync(id);
        if (employee == null)
            return NotFound();

        var employeeResource = _mapper.Map<EmployeeResource>(employee);
        return Ok(employeeResource);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] SaveEmployeeResource resource)
    {
        if (!this.ValidateWithFluent(new SaveEmployeeResourceValidator(), resource))
            return BadRequest(ModelState);

        var departmentExisted = await _departmentsRepository.IsRecordExisted(resource.DepartmentId);
        if (!departmentExisted)
        {
            ModelState.AddModelError(nameof(resource.DepartmentId), "Department does not existed");
            return BadRequest(ModelState);
        }

        var employee = _mapper.Map<Employee>(resource);
        var id = await _employeesRepository.AddAsync(employee);

        return StatusCode(StatusCodes.Status201Created, new CreatedResourceMeta<int>(id));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] SaveEmployeeResource resource)
    {
        if (!this.ValidateWithFluent(new SaveEmployeeResourceValidator(), resource))
            return BadRequest(ModelState);

        var employee = await _employeesRepository.GetByIdAsync(id);
        if (employee == null)
            return NotFound();

        var departmentExisted = await _departmentsRepository.IsRecordExisted(resource.DepartmentId);
        if (!departmentExisted)
        {
            ModelState.AddModelError(nameof(resource.DepartmentId), "Department does not existed");
            return BadRequest(ModelState);
        }

        _mapper.Map(resource, employee);
        employee.DepartmentId = resource.DepartmentId;
        await _employeesRepository.UpdateAsync(employee);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
        var employeeExisted = await _employeesRepository.IsRecordExisted(id);
        if (!employeeExisted)
            return NotFound();

        var employeeFilesPaths = await _employeesFilesRepository.GetFilesPathsAsync(id);
        await _employeesRepository.DeleteAsync(id);

        foreach (var filePath in employeeFilesPaths)
        {
            try
            {
                _filesRepository.Delete(filePath);
            }
            catch (DirectoryNotFoundException) { }
            catch (IOException exception)
            {
                _logger.LogError(exception, $"File: {filePath} could not be deleted " +
                                            $"after delete employee with id: {id}");
            }
        }

        return NoContent();
    }
}