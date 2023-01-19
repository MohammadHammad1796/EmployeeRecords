using EmployeeRecords.Core.Helpers;
using File = EmployeeRecords.Core.Models.File;

namespace EmployeeRecords.Core.Repositories;

public interface IEmployeesFilesRepository : IRepository<File, EmployeeFileQuery, Guid>
{
    Task<IEnumerable<string>> GetFilesPathsAsync(int employeeId);
}