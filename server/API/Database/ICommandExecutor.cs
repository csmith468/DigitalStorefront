namespace API.Database;

/// <summary>
/// Write operations with automatic audit field population if exists on table (CreatedBy/UpdatedBy)
/// </summary>
public interface ICommandExecutor
{
    Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken ct = default);
    Task<int> ExecuteStoredProcedureAsync(string sql, object? parameters = null, CancellationToken ct = default);
    
    /// <returns>ID of inserted database object (like ProductId)</returns>
    Task<int> InsertAsync<T>(T obj, CancellationToken ct = default) where T : class;
    Task BulkInsertAsync<T>(IEnumerable<T> entities, CancellationToken ct = default) where T : class;
    
    Task UpdateAsync<T>(T obj, DateTime? expectedUpdatedAt, CancellationToken ct = default) where T : class;
    Task UpdateFieldAsync<T>(int id, string fieldName, object value, DateTime? expectedUpdatedAt, CancellationToken ct = default) where T : class;
    
    Task DeleteByIdAsync<T>(int id, CancellationToken ct = default) where T : class;
    Task DeleteByFieldAsync<T>(string fieldName, object value, CancellationToken ct = default) where T : class;
    Task DeleteWhereInAsync<T>(string fieldName, List<int> values, CancellationToken ct = default) where T : class;
    Task VerifyConcurrencyAsync<T>(int id, DateTime? expectedUpdatedAt, CancellationToken ct = default) where T : class;
}