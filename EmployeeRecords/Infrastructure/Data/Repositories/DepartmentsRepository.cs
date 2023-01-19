using EmployeeRecords.Core.Helpers;
using EmployeeRecords.Core.Models;
using EmployeeRecords.Core.Repositories;
using System.Data;
using System.Data.SqlClient;

namespace EmployeeRecords.Infrastructure.Data.Repositories;

public class DepartmentsRepository : Repository<Department>, IDepartmentsRepository
{
    public DepartmentsRepository(
        IConfiguration configuration)
        : base(configuration)
    {
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        const string sqlStatement = "SELECT * FROM Departments WHERE Id = @id";
        var idParameter = new SqlParameter("@id", SqlDbType.Int)
        {
            Value = id
        };

        var departments = await ExecuteReaderAsync(sqlStatement, new[] { idParameter }, Map);
        return departments.SingleOrDefault();
    }

    public async Task<Department?> GetByNameAsync(string name)
    {
        const string sqlStatement = "SELECT * FROM Departments WHERE Name = @name";
        var nameParameter = new SqlParameter("@name", SqlDbType.VarChar)
        {
            Value = name
        };

        var departments = await ExecuteReaderAsync(sqlStatement, new[] { nameParameter }, Map);
        return departments.SingleOrDefault();
    }

    public async Task<IEnumerable<Department>> GetAsync(Query? query = null)
    {
        var sqlStatement = "SELECT * FROM Departments";
        if (query != null)
        {
            if (!string.IsNullOrWhiteSpace(query.SearchQuery))
                sqlStatement += " WHERE Name LIKE @searchQuery";
            if (query.Ordering != null)
            {
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

        var departments = await ExecuteReaderAsync(sqlStatement, parameters.ToArray(), Map);
        return departments;
    }

    public async Task<bool> IsRecordExisted(int id)
    {
        const string sqlStatement = "SELECT COUNT(*) FROM Departments WHERE Id = @id";
        var idParameter = new SqlParameter("@id", SqlDbType.Int)
        {
            Value = id
        };

        var count = await ExecuteScalarAsync<int>(sqlStatement, new[] { idParameter });
        return count > 0;
    }

    public async Task<int> GetCountAsync(string? searchQuery = null)
    {
        var sqlStatement = "SELECT COUNT(*) FROM Departments";
        var parameters = new List<SqlParameter>();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            sqlStatement += " WHERE Name LIKE @searchQuery";
            var searchParameter = new SqlParameter("@searchQuery", SqlDbType.VarChar)
            {
                Value = $"%{searchQuery}%"
            };
            parameters.Add(searchParameter);
        }

        var count = await ExecuteScalarAsync<int>(sqlStatement, parameters.ToArray());
        return count;
    }

    public async Task<int> AddAsync(Department department)
    {
        const string sqlStatement = "INSERT INTO Departments (Name) OUTPUT INSERTED.ID VALUES (@name)";
        var nameParameter = new SqlParameter("@name", SqlDbType.VarChar)
        {
            Value = department.Name
        };

        var insertedId = await ExecuteScalarAsync<int>(sqlStatement, new[] { nameParameter });
        return insertedId;
    }

    public async Task UpdateAsync(Department department)
    {
        const string sqlStatement = "UPDATE Departments SET Name = @name WHERE Id = @id";
        var parameters = new SqlParameter[]
        {
            new("@id", SqlDbType.Int)
            {
                Value = department.Id
            },
            new("@name", SqlDbType.VarChar)
            {
                Value = department.Name
            }
        };

        await ExecuteNonQueryAsync(sqlStatement, parameters);
    }

    public async Task DeleteAsync(int id)
    {
        const string sqlStatement = "DELETE FROM Departments WHERE Id = @id";
        var idParameter = new SqlParameter("@id", SqlDbType.Int)
        {
            Value = id
        };

        await ExecuteNonQueryAsync(sqlStatement, new[] { idParameter });
    }

    protected override async Task<IEnumerable<Department>> Map(SqlDataReader reader)
    {
        var departments = new List<Department>();
        while (await reader.ReadAsync())
        {
            var department = new Department
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
            };

            departments.Add(department);
        }
        return departments;
    }
}