using EmployeeRecords.Core.Helpers;
using EmployeeRecords.Core.Models;

namespace EmployeeRecords.Core.Repositories;

public interface IDepartmentsRepository : IRepository<Department, Query, int>
{
    Task<Department?> GetByNameAsync(string name);

    Task<bool> IsRecordExisted(int id);
}