namespace EmployeeRecords.Core.Services;

public interface IEmployeesService
{
    Task<bool> IsDepartmentExistedAsync(int departmentId);
    Task DeleteAsync(int id);
}