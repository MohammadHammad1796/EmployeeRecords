using EmployeeRecords.Core.Helpers;
using EmployeeRecords.Core.Models;

namespace EmployeeRecords.Core.Repositories;

public interface IEmployeesRepository : IRepository<Employee, Query, int>
{
    Task<bool> IsRecordExisted(int id);
}