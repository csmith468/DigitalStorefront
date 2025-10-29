using API.Models.Dtos;

namespace API.Database;

public interface IQueryExecutor
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null);
    Task<T> FirstAsync<T>(string sql, object? parameters = null);
    Task<T?> FirstOrDefaultAsync<T>(string sql, object? parameters = null);
    
    Task<IEnumerable<T>> GetAllAsync<T>() where T : class;
    Task<T?> GetByIdAsync<T>(int id) where T : class;
    Task<IEnumerable<T>> GetWhereAsync<T>(Dictionary<string, object> whereConditions) where T : class;
    Task<IEnumerable<T>> GetByFieldAsync<T>(string fieldName, object value) where T : class;
    Task<IEnumerable<T>> GetWhereInAsync<T>(string fieldName, List<int> values) where T : class;
    Task<IEnumerable<T>> GetWhereInStrAsync<T>(string fieldName, List<string> values) where T : class;

    Task<(IEnumerable<T> items, int totalCount)> GetPaginatedWithSqlAsync<T>(string baseQuery,
        PaginationParams paginationParams,
        object? parameters = null, string? orderByColumn = null, bool descending = true,
        TrustedOrderByExpression? customOrderBy = null) where T : class;
    
    Task<bool> ExistsAsync<T>(int id) where T : class;
    Task<bool> ExistsByFieldAsync<T>(string fieldName, object value) where T : class;
    Task<int> GetCountByFieldAsync<T>(string fieldName, object value) where T : class;
    
    string? Database { get; }
}