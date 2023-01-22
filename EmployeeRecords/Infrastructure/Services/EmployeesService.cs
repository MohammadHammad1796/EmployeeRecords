using EmployeeRecords.Core.Repositories;
using EmployeeRecords.Core.Services;

namespace EmployeeRecords.Infrastructure.Services;

public class EmployeesService : IEmployeesService
{
    private readonly IEmployeesRepository _employeesRepository;
    private readonly IEmployeesFilesRepository _employeesFilesRepository;
    private readonly IFilesRepository _filesRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<EmployeesService> _logger;

    public EmployeesService(
        IEmployeesRepository employeesRepository,
        IEmployeesFilesRepository employeesFilesRepository,
        IFilesRepository filesRepository,
        IDepartmentsRepository departmentsRepository,
        ILogger<EmployeesService> logger)
    {
        _employeesRepository = employeesRepository;
        _employeesFilesRepository = employeesFilesRepository;
        _filesRepository = filesRepository;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
    }

    public async Task<bool> IsDepartmentExistedAsync(int departmentId)
    {
        var departmentExisted = await _departmentsRepository.IsRecordExistedAsync(departmentId);
        return departmentExisted;
    }

    public async Task DeleteAsync(int id)
    {
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
    }
}