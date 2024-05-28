using System.Data.SqlClient;

namespace EmployeeRecords.Infrastructure.Data.Repositories;

public abstract class Repository<TEntity> where TEntity : new()
{
	private readonly string _connectionString;

	protected Repository(IConfiguration configuration)
		=> _connectionString = configuration.GetConnectionString("Default");

	protected async Task<T> ExecuteScalarAsync<T>(string sql, SqlParameter[] parameters)
	{
		await using var connection = new SqlConnection(_connectionString);
		await connection.OpenAsync();
		var command = new SqlCommand(sql, connection);
		command.Parameters.AddRange(parameters);
		var result = await command.ExecuteScalarAsync();
		await connection.CloseAsync();
		return (T)result!;
	}

	protected async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult>(
		string sql, SqlParameter[] parameters,
		Func<SqlDataReader, Task<IEnumerable<TResult>>> map)
	{
		await using var connection = new SqlConnection(_connectionString);
		await connection.OpenAsync();
		var command = new SqlCommand(sql, connection);
		command.Parameters.AddRange(parameters);
		var reader = await command.ExecuteReaderAsync();
		var results = await map(reader);
		await connection.CloseAsync();
		return results;
	}

	protected async Task<IEnumerable<TEntity>> ExecuteReaderAsync(
		string sql, SqlParameter[] parameters)
	{
		return await ExecuteReaderAsync(sql, parameters, Map);
	}

	protected async Task<int> ExecuteNonQueryAsync(string sql, SqlParameter[] parameters)
	{
		await using var connection = new SqlConnection(_connectionString);
		await connection.OpenAsync();
		var command = new SqlCommand(sql, connection);
		command.Parameters.AddRange(parameters);
		var result = await command.ExecuteNonQueryAsync();
		await connection.CloseAsync();
		return result;
	}

	protected abstract Task<IEnumerable<TEntity>> Map(SqlDataReader reader);
}