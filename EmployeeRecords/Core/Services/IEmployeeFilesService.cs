namespace EmployeeRecords.Core.Services;

public interface IEmployeeFilesService
{
    Task<bool> IsEmployeeExistedAsync(int employeeId);
}