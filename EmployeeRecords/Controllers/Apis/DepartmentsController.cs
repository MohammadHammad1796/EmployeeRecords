using AutoMapper;
using EmployeeRecords.Controllers.Apis.Resources;
using EmployeeRecords.Controllers.Validators;
using EmployeeRecords.Core.Models;
using EmployeeRecords.Core.Repositories;
using EmployeeRecords.Custom.Attributes;
using EmployeeRecords.Custom.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeRecords.Controllers.Apis;

[ApiRoute("departments")]
public class DepartmentsController : Controller
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IMapper _mapper;

    public DepartmentsController(
        IDepartmentsRepository departmentsRepository,
        IMapper mapper)
    {
        _departmentsRepository = departmentsRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] DataTableQueryResource resource)
    {
        if (!this.ValidateWithFluent(new DataTableQueryResourceValidator<Department>(), resource))
            return BadRequest(ModelState);

        var query = resource.ToQuery();
        if (!string.IsNullOrWhiteSpace(resource.SearchQuery))
            query.SearchQuery = resource.SearchQuery;

        var departments = await _departmentsRepository.GetAsync(query);
        if (!resource.WithTotal)
            return Ok(departments);

        var total = await _departmentsRepository.GetCountAsync(query.SearchQuery);
        return Ok(new ItemsWithTotalResource<Department>(total, departments));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
    {
        var department = await _departmentsRepository.GetByIdAsync(id);
        return department == null ? NotFound() : Ok(department);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] SaveDepartmentResource resource)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var department = await _departmentsRepository.GetByNameAsync(resource.Name);
        if (department != null)
        {
            ModelState.AddModelError(nameof(resource.Name), "Department with same name existed");
            return BadRequest(ModelState);
        }

        var departmentToInsert = _mapper.Map<Department>(resource);
        var id = await _departmentsRepository.AddAsync(departmentToInsert);

        return StatusCode(StatusCodes.Status201Created, new CreatedResourceMeta<int>(id));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] SaveDepartmentResource resource)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var departmentToUpdate = await _departmentsRepository.GetByIdAsync(id);
        if (departmentToUpdate == null)
            return NotFound();

        var departmentWithSameName = await _departmentsRepository.GetByNameAsync(resource.Name);
        if (departmentWithSameName != null)
        {
            ModelState.AddModelError(nameof(resource.Name), "Department with same name existed");
            return BadRequest(ModelState);
        }

        _mapper.Map(resource, departmentToUpdate);
        await _departmentsRepository.UpdateAsync(departmentToUpdate);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
        var departmentExisted = await _departmentsRepository.IsRecordExistedAsync(id);
        if (!departmentExisted)
            return NotFound();

        await _departmentsRepository.DeleteAsync(id);

        return NoContent();
    }
}