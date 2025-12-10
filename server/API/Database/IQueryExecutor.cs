using API.Models.Dtos;

namespace API.Database;

/// <summary>
/// Read-only data access (used by Dapper)
/// </summary>
public interface IQueryExecutor
{
    // Raw SQL queries
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CancellationToken ct = default);
    Task<T> FirstAsync<T>(string sql, object? parameters = null, CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync<T>(string sql, object? parameters = null, CancellationToken ct = default);
    
    // Attribute-based CRUD (by ID or validated field)
    Task<IEnumerable<T>> GetAllAsync<T>(CancellationToken ct = default) where T : class;
    Task<T?> GetByIdAsync<T>(int id, CancellationToken ct = default) where T : class;
    Task<IEnumerable<T>> GetWhereAsync<T>(Dictionary<string, object> whereConditions, CancellationToken ct = default) where T : class;
    Task<IEnumerable<T>> GetByFieldAsync<T>(string fieldName, object value, CancellationToken ct = default) where T : class;
    Task<IEnumerable<T>> GetWhereInAsync<T>(string fieldName, List<int> values, CancellationToken ct = default) where T : class;
    Task<IEnumerable<T>> GetWhereInStrAsync<T>(string fieldName, List<string> values, CancellationToken ct = default) where T : class;
    Task<bool> ExistsAsync<T>(int id, CancellationToken ct = default) where T : class;
    Task<bool> ExistsByFieldAsync<T>(string fieldName, object value, CancellationToken ct = default) where T : class;
    Task<int> GetCountByFieldAsync<T>(string fieldName, object value, CancellationToken ct = default) where T : class;
    
    /// <summary>
    /// Paginated query with sorting
    /// CustomOrderBy must be explicitly "trusted" to prevent SQL injection
    /// </summary>
    Task<(IEnumerable<T> items, int totalCount)> GetPaginatedWithSqlAsync<T>(string baseQuery,
        PaginationParams paginationParams,
        object? parameters = null, 
        string? orderByColumn = null, 
        bool descending = true,
        TrustedSqlExpression? customOrderBy = null,
        CancellationToken ct = default) where T : class;
    
    string? Database { get; }
}