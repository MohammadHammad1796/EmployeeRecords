using EmployeeRecords.Core.Helpers;
using EmployeeRecords.Core.Repositories;
using System.Data;
using System.Data.SqlClient;
using File = EmployeeRecords.Core.Models.File;

namespace EmployeeRecords.Infrastructure.Data.Repositories;

public class EmployeesFilesRepository : Repository<File>, IEmployeesFilesRepository
{
    public EmployeesFilesRepository(IConfiguration configuration)
        : base(configuration)
    {
    }

    public async Task<File?> GetByIdAsync(Guid id)
    {
        const string sqlStatement = "SELECT * FROM EmployeesFiles ef WHERE ef.Id = @id";
        var idParameter = new SqlParameter("@id", SqlDbType.UniqueIdentifier) { Value = id };

        var files = await ExecuteReaderAsync(sqlStatement, new[] { idParameter }, Map);
        return files.SingleOrDefault();
    }

    public async Task<IEnumerable<File>> GetAsync(EmployeeFileQuery? query = null)
    {
        var sqlStatement = "SELECT ef.* FROM EmployeesFiles ef " +
                           "WHERE ef.EmployeeId = @employeeId ";
        if (query != null)
        {
            if (!string.IsNullOrWhiteSpace(query.SearchQuery))
                sqlStatement += " AND ef.Name LIKE @searchQuery";
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
        if (query != null)
        {
            var employeeIdParameter = new SqlParameter("@employeeId", SqlDbType.Int)
            {
                Value = query.EmployeeId
            };
            parameters.Add(employeeIdParameter);

            if (!string.IsNullOrWhiteSpace(query.SearchQuery))
            {
                var searchParameter = new SqlParameter("@searchQuery", SqlDbType.VarChar)
                {
                    Value = $"%{query.SearchQuery}%"
                };
                parameters.Add(searchParameter);
            }
        }

        var files = await ExecuteReaderAsync(sqlStatement, parameters.ToArray(), Map);
        return files;
    }

    public async Task<int> GetCountAsync(string? searchQuery = null)
    {
        var sqlStatement = "SELECT COUNT(*) FROM EmployeesFiles";
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

    public async Task<IEnumerable<string>> GetFilesPathsAsync(int employeeId)
    {
        const string sqlStatement = "SELECT ef.Path FROM EmployeesFiles ef " +
                                    "WHERE ef.EmployeeId = @employeeId ";

        var parameters = new SqlParameter[]
        {
            new ("@employeeId", SqlDbType.Int) { Value = employeeId }
        };

        var paths = await ExecuteReaderAsync(sqlStatement, parameters.ToArray(), MapPaths);
        return paths;
    }

    public async Task<Guid> AddAsync(File file)
    {
        const string sqlStatement = "INSERT INTO EmployeesFiles (Name, Size, Path, EmployeeId)" +
                                    " OUTPUT INSERTED.ID" +
                                    " VALUES (@name, @size, @path, @employeeId)";
        var parameters = new SqlParameter[]
        {
            new("@name", SqlDbType.VarChar)
            {
                Value = file.Name
            },
            new("@size", SqlDbType.Float)
            {
                Value = file.Size
            },
            new("@path", SqlDbType.VarChar)
            {
                Value = file.Path
            },
            new("@employeeId", SqlDbType.Int)
            {
                Value = file.EmployeeId
            }
        };

        var insertedId = await ExecuteScalarAsync<Guid>(sqlStatement, parameters);
        return insertedId;
    }

    public async Task UpdateAsync(File file)
    {
        const string sqlStatement = "UPDATE EmployeesFiles SET Name = @name, Size = @size, " +
                                    "Path = @path WHERE Id = @id";
        var parameters = new SqlParameter[]
        {
            new("@id", SqlDbType.UniqueIdentifier)
            {
                Value = file.Id
            },
            new("@name", SqlDbType.VarChar)
            {
                Value = file.Name
            },
            new("@size", SqlDbType.Float)
            {
                Value = file.Size
            },
            new("@path", SqlDbType.VarChar)
            {
                Value = file.Path
            }
        };

        await ExecuteNonQueryAsync(sqlStatement, parameters);
    }

    public async Task DeleteAsync(Guid id)
    {
        const string sqlStatement = "DELETE FROM EmployeesFiles WHERE Id = @id";
        var idParameter = new SqlParameter("@id", SqlDbType.UniqueIdentifier)
        {
            Value = id
        };

        await ExecuteNonQueryAsync(sqlStatement, new[] { idParameter });
    }

    protected override async Task<IEnumerable<File>> Map(SqlDataReader reader)
    {
        var files = new List<File>();
        while (await reader.ReadAsync())
        {
            var file = new File
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Size = reader.GetDouble(reader.GetOrdinal("Size")),
                Path = reader.GetString(reader.GetOrdinal("Path")),
                EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId"))
            };

            files.Add(file);
        }
        return files;
    }

    private static async Task<IEnumerable<string>> MapPaths(SqlDataReader reader)
    {
        var paths = new List<string>();
        while (await reader.ReadAsync())
        {
            var path = reader.GetString(reader.GetOrdinal("Path"));
            paths.Add(path);
        }
        return paths;
    }
}