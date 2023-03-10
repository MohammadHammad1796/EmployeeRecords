using AutoMapper;
using EmployeeRecords.Controllers.Apis.Resources;
using EmployeeRecords.Controllers.Validators;
using EmployeeRecords.Core.Helpers;
using EmployeeRecords.Core.Repositories;
using EmployeeRecords.Core.Services;
using EmployeeRecords.Custom.Attributes;
using EmployeeRecords.Custom.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using File = EmployeeRecords.Core.Models.File;

namespace EmployeeRecords.Controllers.Apis;

[ApiRoute("employees/{employeeId:int}/files")]
public class EmployeesFilesController : Controller
{
    private readonly IEmployeesFilesRepository _employeesFilesRepository;
    private readonly IEmployeeFilesService _employeeFilesService;
    private readonly IFilesRepository _filesRepository;
    private readonly ILogger<EmployeesFilesController> _logger;
    private readonly FileCriteria _fileCriteria;
    private readonly IMapper _mapper;
    private const string EmployeesFilesPath = "employees";

    public EmployeesFilesController(
        IEmployeesFilesRepository employeesFilesRepository,
        IEmployeeFilesService employeeFilesService,
        IFilesRepository filesRepository,
        ILogger<EmployeesFilesController> logger,
        IMapper mapper,
        IOptions<FileCriteria> fileCriteraiOptions)
    {
        _employeesFilesRepository = employeesFilesRepository;
        _employeeFilesService = employeeFilesService;
        _mapper = mapper;
        _filesRepository = filesRepository;
        _logger = logger;
        _fileCriteria = fileCriteraiOptions.Value;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(
        [FromRoute] int employeeId,
        [FromQuery] DataTableQueryResource resource)
    {
        if (!this.ValidateWithFluent(new DataTableQueryResourceValidator<FileResource>(), resource))
            return BadRequest(ModelState);

        var employeeExisted = await _employeeFilesService.IsEmployeeExistedAsync(employeeId);
        if (!employeeExisted)
            return NotFound();

        var query = resource.ToFileQuery();
        if (!string.IsNullOrWhiteSpace(resource.SearchQuery))
            query.SearchQuery = resource.SearchQuery;
        query.EmployeeId = employeeId;

        var files = await _employeesFilesRepository.GetAsync(query);
        var fileResources = _mapper.Map<IEnumerable<FileResource>>(files);
        if (!resource.WithTotal)
            return Ok(fileResources);

        var total = await _employeesFilesRepository.GetCountForEmployeeAsync(employeeId, query.SearchQuery);
        return Ok(new ItemsWithTotalResource<FileResource>(total, fileResources));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] int employeeId,
        [FromRoute] Guid id)
    {
        var employeeExisted = await _employeeFilesService.IsEmployeeExistedAsync(employeeId);
        if (!employeeExisted)
            return NotFound();

        var file = await _employeesFilesRepository.GetByIdAsync(id);
        if (file == null)
            return NotFound();

        var fileResource = _mapper.Map<FileResource>(file);
        return Ok(fileResource);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromRoute] int employeeId,
        [FromForm] AddEmployeeFileResource resource)
    {
        if (!this.ValidateWithFluent(new AddEmployeeFileValidator(_fileCriteria), resource))
            return BadRequest(ModelState);

        var employeeExisted = await _employeeFilesService.IsEmployeeExistedAsync(employeeId);
        if (!employeeExisted)
            return NotFound();

        string filePath;
        try
        {
            filePath = await _filesRepository.SaveAsync(resource.File, EmployeesFilesPath);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "File could not be saved " +
                                        $"after try to create employee file, employee id: {employeeId}");
            return this.ServerError();
        }

        var sizeInKb = (double)resource.File.Length / 1024;
        var sizeInMb = sizeInKb / 1024;
        sizeInMb = Math.Round(sizeInMb, 2);
        var file = new File
        {
            Size = sizeInMb,
            Name = resource.Name,
            Path = filePath,
            EmployeeId = employeeId
        };
        var id = await _employeesFilesRepository.AddAsync(file);

        return StatusCode(StatusCodes.Status201Created, new CreatedFileResource(filePath, id));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] int employeeId,
        [FromRoute] Guid id,
        [FromForm] UpdateEmployeeFileResource resource)
    {
        if (!this.ValidateWithFluent(new UpdateEmployeeFileValidator(_fileCriteria), resource))
            return BadRequest(ModelState);

        var employeeExisted = await _employeeFilesService.IsEmployeeExistedAsync(employeeId);
        if (!employeeExisted)
            return NotFound();

        var file = await _employeesFilesRepository.GetByIdAsync(id);
        if (file == null)
            return NotFound();

        file.Name = resource.Name;
        if (resource.File == null)
        {
            await _employeesFilesRepository.UpdateAsync(file);
            return NoContent();
        }

        string? newFilePath;
        try
        {
            newFilePath = await _filesRepository.SaveAsync(resource.File, EmployeesFilesPath);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "File could not be saved " +
                                        $"after try to upload new one, record id: {id}");
            return this.ServerError();
        }

        var sizeInKb = (double)resource.File.Length / 1024;
        var sizeInMb = sizeInKb / 1024;
        sizeInMb = Math.Round(sizeInMb, 2);
        file.Size = sizeInMb;
        var oldFilePath = file.Path;
        file.Path = newFilePath;
        await _employeesFilesRepository.UpdateAsync(file);

        try
        {
            _filesRepository.Delete(oldFilePath);
        }
        catch (IOException exception)
        {
            _logger.LogError(exception, $"File: {file.Path} could not be deleted " +
                                        $"after upload new one, record id: {id}");
        }

        return Ok(new UpdatedFileResource(newFilePath));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] int employeeId,
        [FromRoute] Guid id)
    {
        var employeeExisted = await _employeeFilesService.IsEmployeeExistedAsync(employeeId);
        if (!employeeExisted)
            return NotFound();

        var file = await _employeesFilesRepository.GetByIdAsync(id);
        if (file == null)
            return NotFound();

        await _employeesFilesRepository.DeleteAsync(id);

        try
        {
            _filesRepository.Delete(file.Path);
        }
        catch (DirectoryNotFoundException) { }
        catch (IOException exception)
        {
            _logger.LogError(exception, $"File: {file.Path} could not be deleted " +
                                        "after delete his record from db");
        }

        return NoContent();
    }
}