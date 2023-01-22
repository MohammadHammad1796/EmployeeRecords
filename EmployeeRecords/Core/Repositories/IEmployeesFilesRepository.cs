using EmployeeRecords.Core.Helpers;
using File = EmployeeRecords.Core.Models.File;

namespace EmployeeRecords.Core.Repositories;

public interface IEmployeesFilesRepository : IRepository<File, EmployeeFileQuery, Guid>
{
    Task<int> GetCountForEmployeeAsync(int employeeId, string? searchQuery = null);
    Task<IEnumerable<string>> GetFilesPathsAsync(int employeeId);
}