using EmployeeRecords.Core.Helpers;

namespace EmployeeRecords.Core.Repositories;

public interface IRepository<TEntity, TQuery, TKey>
    where TEntity : class
    where TQuery : Query
{
    Task<TEntity?> GetByIdAsync(TKey id);

    Task<IEnumerable<TEntity>> GetAsync(TQuery? query = null);

    Task<int> GetCountAsync(string? searchQuery = null);

    Task<TKey> AddAsync(TEntity entity);

    Task UpdateAsync(TEntity entity);

    Task DeleteAsync(TKey id);
}