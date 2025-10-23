using System.Data;
using API.Models.Dtos;
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
    Task DeleteByIdAsync<T>(int id) where T : class;
    
    Task<IEnumerable<T>> GetAllAsync<T>() where T : class;
    Task<T?> GetByIdAsync<T>(int id) where T : class;
    Task<IEnumerable<T>> GetWhereAsync<T>(Dictionary<string, object> whereConditions) where T : class;
    Task<IEnumerable<T>> GetByFieldAsync<T>(string fieldName, object value) where T : class;
    Task<IEnumerable<T>> GetWhereInAsync<T>(string fieldName, List<int> values) where T : class;
    Task<(IEnumerable<T> items, int totalCount)> GetPaginatedWithSqlAsync<T>(string baseQuery, 
        PaginationParams paginationParams, object? parameters = null, string orderBy = "1 DESC") where T : class;
    Task<bool> ExistsAsync<T>(int id) where T : class;
    Task<bool> ExistsByFieldAsync<T>(string fieldName, object value) where T : class;
    Task<int> GetCountByFieldAsync<T>(string fieldName, object value) where T : class;
    
    string? Database { get; }
    IDbTransaction? Transaction { get; }
}

public class DataContextDapper : IDataContextDapper
{
      private readonly IDbConnection _db;
      private IDbTransaction? _transaction;
      private readonly IHttpContextAccessor _httpContextAccessor;

      public DataContextDapper(IConfiguration config, IHttpContextAccessor httpContextAccessor)
      {
          _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
          _db.Open();
          _httpContextAccessor = httpContextAccessor;
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
          
          if (metadata.Columns.Any(c => c.Name == "CreatedAt"))
              obj = SetEntityProp(obj, "CreatedAt", DateTime.UtcNow);
          
          var userId = GetCurrentUserId();
          if (userId != null && metadata.Columns.Any(c => c.Name == "CreatedBy"))
              obj = SetEntityProp(obj, "CreatedBy", userId.Value);
          
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

          if (metadata.Columns.Any(c => c.Name == "UpdatedAt"))
              obj = SetEntityProp(obj, "UpdatedAt", DateTime.UtcNow);

          var userId = GetCurrentUserId();
          if (userId != null && metadata.Columns.Any(c => c.Name == "UpdatedBy"))
              obj = SetEntityProp(obj, "UpdatedBy", userId.Value);
          
          var columnUpdates = string.Join(",", metadata.Columns.Select(c => $"[{c.Name}] = @{c.Name}"));
          var parameters = DbAttributes.CreateParameters(obj, metadata.Columns);
          parameters[metadata.PrimaryKey.Name] = metadata.PrimaryKey.GetValue(obj)!;
          
          var sql = $"UPDATE {metadata.TableName} SET {columnUpdates} WHERE [{metadata.PrimaryKey.Name}] = @{metadata.PrimaryKey.Name}";
          await ExecuteAsync(sql, parameters);
      }

      public async Task DeleteByIdAsync<T>(int id) where T : class
      {
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"DELETE FROM {metadata.TableName} WHERE [{metadata.PrimaryKey.Name}] = @id";
          await ExecuteAsync(sql, new { id });
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
          if (string.IsNullOrWhiteSpace(fieldName))
              throw new ArgumentException("Field name is required", nameof(fieldName));
          return await GetWhereAsync<T>(new Dictionary<string, object>{ { fieldName, value } });
      }

      public async Task<IEnumerable<T>> GetWhereInAsync<T>(string fieldName, List<int> values) where T : class
      {
          if (string.IsNullOrWhiteSpace(fieldName))
              throw new ArgumentException("Field name is required", nameof(fieldName));
          
          if (values.Count == 0)
              return [];
          
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"SELECT * FROM {metadata.TableName} WHERE [{fieldName}] IN @values";
    
          return await QueryAsync<T>(sql, new { values });
      }

      public async Task<(IEnumerable<T> items, int totalCount)> GetPaginatedWithSqlAsync<T>(string baseQuery,
          PaginationParams paginationParams, object? parameters = null, string orderBy = "1 DESC") where T : class
      {
          var countSql = $"SELECT COUNT(*) FROM ({baseQuery}) AS CountQuery";
          Console.WriteLine($"COUNT SQL: {countSql}");
          var totalCount = await _db.ExecuteScalarAsync<int>(countSql, parameters, _transaction);
          Console.WriteLine($"Total Count: {totalCount}");

          var paginatedSql = $"""
                              {baseQuery}
                              ORDER BY {orderBy}
                              OFFSET @skip ROWS
                              FETCH NEXT @pageSize ROWS ONLY
                              """;
          Console.WriteLine($"PAGINATED SQL: {paginatedSql}");
          Console.WriteLine($"Skip: {paginationParams.Skip}, PageSize: {paginationParams.PageSize}");

          var allParms = new DynamicParameters(parameters);
          allParms.Add("skip", paginationParams.Skip);
          allParms.Add("pageSize", paginationParams.PageSize);

          var items = await _db.QueryAsync<T>(paginatedSql, allParms);
          Console.WriteLine($"Items returned: {items.Count()}");
          return (items, totalCount);
      }

      public async Task<bool> ExistsAsync<T>(int id) where T : class
      {
          return await GetByIdAsync<T>(id) != null;
      }

      public async Task<bool> ExistsByFieldAsync<T>(string fieldName, object value) where T : class
      {
          return (await GetWhereAsync<T>(new Dictionary<string, object> { { fieldName, value } })).Any();
      }

      public async Task<int> GetCountByFieldAsync<T>(string fieldName, object value) where T : class
      {
          return (await GetWhereAsync<T>(new Dictionary<string, object> { { fieldName, value } })).Count();
      }
      
      private int? GetCurrentUserId()
      {
          var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
          return userIdClaim != null ? int.Parse(userIdClaim!) : null;
      }

      private T SetEntityProp<T>(T obj, string field, object value) where T : class
      {
          var type = typeof(T);
          var prop = type.GetProperty(field);
          if (prop == null) return obj;

          prop.SetValue(obj, value);
          return obj;
      }


      public IDbTransaction? Transaction => _transaction;
      
      public string Database => _db.Database;


      public virtual void Dispose()
      {
          _transaction?.Dispose();
          _db.Dispose();
      }
}