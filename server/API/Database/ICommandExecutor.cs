namespace API.Database;

public interface ICommandExecutor
{
    Task<int> ExecuteAsync(string sql, object? parameters = null);
    Task<int> ExecuteStoredProcedureAsync(string sql, object? parameters = null);
    Task<int> InsertAsync<T>(T obj) where T : class;
    Task UpdateAsync<T>(T obj) where T : class;
    Task UpdateFieldAsync<T>(int id, string fieldName, object value) where T : class;
    Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : class;
    Task DeleteByIdAsync<T>(int id) where T : class;
    Task DeleteByFieldAsync<T>(string fieldName, object value) where T : class;
    Task DeleteWhereInAsync<T>(string fieldName, List<int> values) where T : class;
}