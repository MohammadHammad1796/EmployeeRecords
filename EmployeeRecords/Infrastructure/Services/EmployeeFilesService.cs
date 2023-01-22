using EmployeeRecords.Core.Repositories;
using EmployeeRecords.Core.Services;

namespace EmployeeRecords.Infrastructure.Services;

public class EmployeeFilesService : IEmployeeFilesService
{
    private readonly IEmployeesRepository _employeesRepository;

    public EmployeeFilesService(IEmployeesRepository employeesRepository)
    {
        _employeesRepository = employeesRepository;
    }

    public async Task<bool> IsEmployeeExistedAsync(int employeeId)
    {
        var employeeExisted = await _employeesRepository.IsRecordExistedAsync(employeeId);
        return employeeExisted;
    }
}