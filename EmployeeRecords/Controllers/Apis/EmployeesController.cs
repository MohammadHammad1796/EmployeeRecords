using AutoMapper;
using EmployeeRecords.Controllers.Apis.Resources;
using EmployeeRecords.Controllers.Validators;
using EmployeeRecords.Core.Models;
using EmployeeRecords.Core.Repositories;
using EmployeeRecords.Core.Services;
using EmployeeRecords.Custom.Attributes;
using EmployeeRecords.Custom.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeRecords.Controllers.Apis;

[ApiRoute("employees")]
public class EmployeesController : Controller
{
    private readonly IEmployeesRepository _employeesRepository;
    private readonly IEmployeesService _employeesService;

    private readonly IMapper _mapper;

    public EmployeesController(
        IEmployeesService employeesService,
        IEmployeesRepository employeesRepository,
        IMapper mapper)
    {
        _employeesRepository = employeesRepository;
        _employeesService = employeesService;
        _mapper = mapper;
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

        var departmentExisted = await _employeesService.IsDepartmentExistedAsync(resource.DepartmentId);
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

        var departmentExisted = await _employeesService.IsDepartmentExistedAsync(resource.DepartmentId);
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
        var employeeExisted = await _employeesRepository.IsRecordExistedAsync(id);
        if (!employeeExisted)
            return NotFound();

        await _employeesService.DeleteAsync(id);

        return NoContent();
    }
}