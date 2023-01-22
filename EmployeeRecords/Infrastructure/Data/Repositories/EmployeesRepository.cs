using EmployeeRecords.Core.Helpers;
using EmployeeRecords.Core.Models;
using EmployeeRecords.Core.Repositories;
using System.Data;
using System.Data.SqlClient;

namespace EmployeeRecords.Infrastructure.Data.Repositories;

public class EmployeesRepository : Repository<Employee>, IEmployeesRepository
{
    public EmployeesRepository(IConfiguration configuration)
        : base(configuration)
    {
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        const string sqlStatement = "SELECT e.*, d.Name AS DepartmentName FROM Employees e " +
                                    "INNER JOIN Departments d ON e.DepartmentId = d.Id " +
                                    "WHERE e.Id = @id";
        var idParameter = new SqlParameter("@id", SqlDbType.Int) { Value = id };
        var employees = await ExecuteReaderAsync(
            sqlStatement, new[] { idParameter }, Map);

        return employees.SingleOrDefault();
    }

    public async Task<bool> IsRecordExistedAsync(int id)
    {
        const string sqlStatement = "SELECT COUNT(*) FROM Employees WHERE Id = @id";
        var idParameter = new SqlParameter("@id", SqlDbType.Int)
        {
            Value = id
        };
        var count = await ExecuteScalarAsync<int>(sqlStatement, new[] { idParameter });
        return count > 0;
    }

    public async Task<IEnumerable<Employee>> GetAsync(Query? query = null)
    {
        var sqlStatement = "SELECT e.*, d.Name AS DepartmentName FROM Employees e " +
                           "INNER JOIN Departments d ON e.DepartmentId = d.Id";
        if (query != null)
        {
            if (!string.IsNullOrWhiteSpace(query.SearchQuery))
                sqlStatement += " WHERE e.Name LIKE @searchQuery OR d.Name LIKE @searchQuery";
            if (query.Ordering != null)
            {
                if (query.Ordering.By.ToLower() == "department")
                    query.Ordering.By = "d.Name";

                sqlStatement +=
                    $@" ORDER BY {query.Ordering.By}";
                if (!query.Ordering.IsAscending)
                    sqlStatement += " desc";
            }
            if (query.Pagination != null)
                sqlStatement += $" OFFSET {(query.Pagination.Number - 1) * query.Pagination.Size} " +
                                $"ROWS FETCH NEXT {query.Pagination.Size} ROWS ONLY";
        }
        var parameters = new List<SqlParameter>();
        if (!string.IsNullOrWhiteSpace(query?.SearchQuery))
        {
            var searchParameter = new SqlParameter("@searchQuery", SqlDbType.VarChar)
            {
                Value = $"%{query.SearchQuery}%"
            };
            parameters.Add(searchParameter);
        }

        var employees = await ExecuteReaderAsync(sqlStatement,
            parameters.ToArray(), Map);

        return employees;
    }

    public async Task<int> GetCountAsync(string? searchQuery = null)
    {
        var sqlStatement = "SELECT COUNT(e.Id) FROM Employees e INNER JOIN Departments d" +
                           " ON e.DepartmentId = d.Id";
        var parameters = new List<SqlParameter>();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            sqlStatement += " WHERE e.Name LIKE @searchQuery OR d.Name LIKE @searchQuery";
            var searchParameter = new SqlParameter("@searchQuery", SqlDbType.VarChar)
            {
                Value = $"%{searchQuery}%"
            };
            parameters.Add(searchParameter);
        }

        var count = await ExecuteScalarAsync<int>(sqlStatement, parameters.ToArray());
        return count;
    }

    public async Task<int> AddAsync(Employee employee)
    {
        const string sqlStatement = "INSERT INTO Employees (Name, DateOfBirth, Address, DepartmentId) OUTPUT INSERTED.ID" +
                                    " VALUES (@name, @dateOfBirth, @address, @departmentId)";
        var parameters = new SqlParameter[]
        {
            new("@name", SqlDbType.VarChar)
            {
                Value = employee.Name
            },
            new("@dateOfBirth", SqlDbType.Date)
            {
                Value = employee.DateOfBirth
            },
            new("@address", SqlDbType.VarChar)
            {
                Value = employee.Address
            },
            new("@departmentId", SqlDbType.Int)
            {
                Value = employee.DepartmentId
            }
        };

        var insertedId = await ExecuteScalarAsync<int>(sqlStatement, parameters);
        return insertedId;
    }

    public async Task UpdateAsync(Employee employee)
    {
        const string sqlStatement = "UPDATE Employees SET Name = @name, DateOfBirth = @dateOfBirth, " +
                                    "Address = @address, DepartmentId = @departmentId WHERE Id = @id";
        var parameters = new SqlParameter[]
        {
            new("@id", SqlDbType.VarChar)
            {
                Value = employee.Id
            },
            new("@name", SqlDbType.VarChar)
            {
                Value = employee.Name
            },
            new("@dateOfBirth", SqlDbType.Date)
            {
                Value = employee.DateOfBirth
            },
            new("@address", SqlDbType.VarChar)
            {
                Value = employee.Address
            },
            new("@departmentId", SqlDbType.Int)
            {
                Value = employee.DepartmentId
            }
        };

        await ExecuteNonQueryAsync(sqlStatement, parameters);
    }

    public async Task DeleteAsync(int id)
    {
        const string sqlStatement = "DELETE FROM Employees WHERE Id = @id";
        var idParameter = new SqlParameter("@id", SqlDbType.Int)
        {
            Value = id
        };

        await ExecuteNonQueryAsync(sqlStatement, new[] { idParameter });
    }

    protected override async Task<IEnumerable<Employee>> Map(SqlDataReader reader)
    {
        var employees = new List<Employee>();
        while (await reader.ReadAsync())
        {
            var employee = new Employee
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                Address = reader.GetString(reader.GetOrdinal("Address")),
                Department = new Department
                {
                    Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                    Name = reader.GetString(reader.GetOrdinal("DepartmentName"))
                }
            };

            employees.Add(employee);
        }
        return employees;
    }
}