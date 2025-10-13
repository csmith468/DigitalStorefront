using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace API.Database;

public interface IDataContextDapper : IDisposable
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null);
    Task<T> FirstAsync<T>(string sql, object? parameters = null);
    Task<T?> FirstOrDefaultAsync<T>(string sql, object? parameters = null);

    Task<int> ExecuteAsync(string sql, object? parameters = null);
    Task<int> ExecuteStoredProcedureAsync(string sql, object? parameters = null);

    Task<T> WithTransactionAsync<T>(Func<Task<T>> func);
    Task WithTransactionAsync(Func<Task> func);

    Task<int> InsertAsync<T>(T obj) where T : class;
    Task UpdateAsync<T>(T obj) where T : class;
    Task<IEnumerable<T>> GetAllAsync<T>() where T : class;
    Task<T?> GetByIdAsync<T>(int id) where T : class;
    Task<IEnumerable<T>> GetWhereAsync<T>(Dictionary<string, object> whereConditions) where T : class;
    Task<IEnumerable<T>> GetByFieldAsync<T>(string fieldName, object value) where T : class;
    Task<bool> ExistsAsync<T>(int id) where T : class;
    Task<bool> ExistsByFieldAsync<T>(string fieldName, object value) where T : class;
    
    string? Database { get; }
    IDbTransaction? Transaction { get; }
}

public class DataContextDapper : IDataContextDapper
{
      private readonly IDbConnection _db;
      private IDbTransaction? _transaction;

      public DataContextDapper(IConfiguration config)
      {
          _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
          _db.Open();
      }

      public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
      {
          return await _db.QueryAsync<T>(sql, parameters, _transaction);
      }

      public async Task<T> FirstAsync<T>(string sql, object? parameters = null)
      {
          return await _db.QueryFirstAsync<T>(sql, parameters, _transaction);
      }
      
      public async Task<T?> FirstOrDefaultAsync<T>(string sql, object? parameters = null)
      {
          return await _db.QueryFirstOrDefaultAsync<T>(sql, parameters, _transaction);
      }

      public async Task<int> ExecuteAsync(string sql, object? parameters = null)
      {
          return await _db.ExecuteAsync(sql, parameters, _transaction);
      }

      public async Task<int> ExecuteStoredProcedureAsync(string sql, object? parameters = null)
      {
          return await _db.ExecuteAsync(sql, parameters, _transaction, commandType: CommandType.StoredProcedure);
      }

      public async Task<T> WithTransactionAsync<T>(Func<Task<T>> func)
      {
          if (_transaction != null) return await func();
          
          try
          {
              _transaction = _db.BeginTransaction();
              var result = await func();
              _transaction.Commit();
              _transaction = null;
              return result;
          }
          catch
          {
              _transaction?.Rollback();
              _transaction = null;
              throw;
          }
      }

      public async Task WithTransactionAsync(Func<Task> func)
      {
          await WithTransactionAsync<object?>(async () =>
          {
              await func();
              return null;
          });
      }

      public async Task<int> InsertAsync<T>(T obj) where T : class
      {
          var metadata = DbAttributes.GetTableMetadata<T>();
          
          var columnNames = string.Join(",", metadata.Columns.Select(c => $"[{c.Name}]"));
          var parameterNames = string.Join(",", metadata.Columns.Select(c => $"@{c.Name}"));
          
          var parameters = DbAttributes.CreateParameters(obj, metadata.Columns);
          var sql = $"INSERT INTO {metadata.TableName} ({columnNames}) VALUES ({parameterNames}); SELECT CAST(SCOPE_IDENTITY() AS INT);";
          
          var result = (await QueryAsync<int>(sql, parameters)).FirstOrDefault();
          return result == 0 ? throw new InvalidOperationException("Insert failed - no identity returned") : result;
      }

      public async Task UpdateAsync<T>(T obj) where T : class
      {
          var metadata = DbAttributes.GetTableMetadata<T>();
          
          var columnUpdates = string.Join(",", metadata.Columns.Select(c => $"[{c.Name}] = @{c.Name}"));
          var parameters = DbAttributes.CreateParameters(obj, metadata.Columns);
          parameters[metadata.PrimaryKey.Name] = metadata.PrimaryKey.GetValue(obj)!;
          
          var sql = $"UPDATE {metadata.TableName} SET {columnUpdates} WHERE [{metadata.PrimaryKey.Name}] = @{metadata.PrimaryKey.Name}";
          await ExecuteAsync(sql, parameters);
      }

      public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class
      {
          var metadata = DbAttributes.GetTableMetadata<T>();
          return await QueryAsync<T>($"SELECT * FROM {metadata.TableName}");
      }

      public async Task<T?> GetByIdAsync<T>(int id) where T : class
      {
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"SELECT * FROM {metadata.TableName} WHERE [{metadata.PrimaryKey.Name}] = @id";
          return await FirstOrDefaultAsync<T>(sql, new { id });
      }

      public async Task<IEnumerable<T>> GetWhereAsync<T>(Dictionary<string, object> whereConditions) where T : class
      {
          if (whereConditions == null || !whereConditions.Any())
              throw new ArgumentException("At least one condition is required");
          
          var metadata = DbAttributes.GetTableMetadata<T>();
          var whereClause = string.Join(" AND ", whereConditions.Keys.Select(key => $"[{key}] = @{key}"));
          var sql = $"SELECT * FROM {metadata.TableName} WHERE {whereClause}";
          return await QueryAsync<T>(sql, whereConditions);
      }

      public async Task<IEnumerable<T>> GetByFieldAsync<T>(string fieldName, object value) where T : class
      {
          return await GetWhereAsync<T>(new Dictionary<string, object>{ { fieldName, value } });
      }

      public async Task<bool> ExistsAsync<T>(int id) where T : class
      {
          return await GetByIdAsync<T>(id) != null;
      }

      public async Task<bool> ExistsByFieldAsync<T>(string fieldName, object value) where T : class
      {
          return (await GetWhereAsync<T>(new Dictionary<string, object> { { fieldName, value } })).Any();
      }


      public IDbTransaction? Transaction => _transaction;
      
      public string Database => _db.Database;


      public virtual void Dispose()
      {
          _transaction?.Dispose();
          _db.Dispose();
      }
}