using System.Data;
using API.Models.Dtos;
using API.Services;
using API.Services.Contexts;
using Microsoft.Data.SqlClient;
using Dapper;

namespace API.Database;

public class DataContextDapper : IQueryExecutor, ICommandExecutor, ITransactionManager, IDisposable, IAsyncDisposable
{
      private readonly SqlConnection _db;
      private SqlTransaction? _transaction;
      private IAuditContext _auditContext;
      private bool _disposed;

      // NOTE: IAuditContext auto-populates Createdby/UpdatedBy fields on inserts/updates
      // This abstracts the user source (HTTP vs system user) so the data layer doesn't depend on ASP.NET
      // Keeps audit tracking DRY without pass userId everywhere or relying on people to remember to set fields
      
      // TODO for production: Add CancellationToken params to allow query cancellation when clients disconnect
      public DataContextDapper(IConfiguration config, IAuditContext auditContext)
      {
          _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
          _auditContext = auditContext;
      }

      // IQueryExecutor Implementations
      public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
      {
          EnsureConnectionOpen();
          return await _db.QueryAsync<T>(sql, parameters, _transaction);
      }

      public async Task<T> FirstAsync<T>(string sql, object? parameters = null)
      {
          EnsureConnectionOpen();
          return await _db.QueryFirstAsync<T>(sql, parameters, _transaction);
      }
      
      public async Task<T?> FirstOrDefaultAsync<T>(string sql, object? parameters = null)
      {
          EnsureConnectionOpen();
          return await _db.QueryFirstOrDefaultAsync<T>(sql, parameters, _transaction);
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

          foreach (var key in whereConditions.Keys.Where(key => !DbAttributes.ValidateColumnExists<T>(key)))
              throw new ArgumentException($"Field '{key}' not found", nameof(whereConditions));

          var metadata = DbAttributes.GetTableMetadata<T>();
          var whereClause = string.Join(" AND ", whereConditions.Keys.Select(key => $"[{key}] = @{key}"));
          var sql = $"SELECT * FROM {metadata.TableName} WHERE {whereClause}";
          return await QueryAsync<T>(sql, whereConditions);
      }

      public async Task<IEnumerable<T>> GetByFieldAsync<T>(string fieldName, object value) where T : class
      {
          ValidateFieldName<T>(fieldName);
          return await GetWhereAsync<T>(new Dictionary<string, object>{ { fieldName, value } });
      }

      public async Task<IEnumerable<T>> GetWhereInAsync<T>(string fieldName, List<int> values) where T : class
      {
          ValidateFieldName<T>(fieldName);
          if (values.Count == 0) return [];
          
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"SELECT * FROM {metadata.TableName} WHERE [{fieldName}] IN @values";
    
          return await QueryAsync<T>(sql, new { values });
      }
      public async Task<IEnumerable<T>> GetWhereInStrAsync<T>(string fieldName, List<string> values) where T : class
      {
          ValidateFieldName<T>(fieldName);
          if (values.Count == 0) return [];
          
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"SELECT * FROM {metadata.TableName} WHERE [{fieldName}] IN @values";
    
          return await QueryAsync<T>(sql, new { values });
      }

      public async Task<(IEnumerable<T> items, int totalCount)> GetPaginatedWithSqlAsync<T>(
          string baseQuery,
          PaginationParams paginationParams,
          object? parameters = null,
          string? orderByColumn = null,
          bool descending = true,
          TrustedOrderByExpression? customOrderBy = null) where T : class
      {
          EnsureConnectionOpen();
          var countSql = $"SELECT COUNT(*) FROM ({baseQuery}) AS CountQuery";
          var totalCount = await _db.ExecuteScalarAsync<int>(countSql, parameters, _transaction);

          string orderBy;

          if (customOrderBy != null)
          {
              // Order by must be marked as "Trusted" in function calling GetPaginatedWithSqlAsync
              // or it will fail (this is not marked as trusted because calling this function should not
              // automatically trust the 
              orderBy = customOrderBy.ToSql();
          }
          else if (!string.IsNullOrWhiteSpace(orderByColumn))
          {
              if (!DbAttributes.ValidateColumnExists<T>(orderByColumn))
                  throw new ArgumentException($"Invalid sort column: {orderByColumn}", nameof(orderByColumn));

              var direction = descending ? "DESC" : "ASC";
              orderBy = $"[{orderByColumn}] {direction}";
          }
          else orderBy = "1 DESC";

          var paginatedSql = $"""
                              {baseQuery}
                              ORDER BY {orderBy}
                              OFFSET @skip ROWS
                              FETCH NEXT @pageSize ROWS ONLY
                              """;

          var allParms = new DynamicParameters(parameters);
          allParms.Add("skip", paginationParams.Skip);
          allParms.Add("pageSize", paginationParams.PageSize);

          var items = await _db.QueryAsync<T>(paginatedSql, allParms);
          return (items, totalCount);
      }

      public async Task<bool> ExistsAsync<T>(int id) where T : class
      {
          return await GetByIdAsync<T>(id) != null;
      }

      public async Task<bool> ExistsByFieldAsync<T>(string fieldName, object value) where T : class
      {
          if (!DbAttributes.ValidateColumnExists<T>(fieldName))
              throw new ArgumentException("Field not found", nameof(fieldName));
          return (await GetWhereAsync<T>(new Dictionary<string, object> { { fieldName, value } })).Any();
      }

      public async Task<int> GetCountByFieldAsync<T>(string fieldName, object value) where T : class
      {
          if (!DbAttributes.ValidateColumnExists<T>(fieldName))
              throw new ArgumentException("Field not found", nameof(fieldName));
          return (await GetWhereAsync<T>(new Dictionary<string, object> { { fieldName, value } })).Count();
      }
      
      
      // ICommandExecutor Implementations
      public async Task<int> ExecuteAsync(string sql, object? parameters = null)
      {
          EnsureConnectionOpen();
          return await _db.ExecuteAsync(sql, parameters, _transaction);
      }

      public async Task<int> ExecuteStoredProcedureAsync(string sql, object? parameters = null)
      {
          EnsureConnectionOpen();
          return await _db.ExecuteAsync(sql, parameters, _transaction, commandType: CommandType.StoredProcedure);
      }

      public async Task<int> InsertAsync<T>(T obj) where T : class
      {
          var metadata = DbAttributes.GetTableMetadata<T>();
          
          if (metadata.Columns.Any(c => c.Name == "CreatedAt"))
              obj = SetEntityProp(obj, "CreatedAt", DateTime.UtcNow);
          
          var userId = _auditContext.UserId;
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

          var userId = _auditContext.UserId;
          if (userId != null && metadata.Columns.Any(c => c.Name == "UpdatedBy"))
              obj = SetEntityProp(obj, "UpdatedBy", userId.Value);
          
          var columnUpdates = string.Join(",", metadata.Columns.Select(c => $"[{c.Name}] = @{c.Name}"));
          var parameters = DbAttributes.CreateParameters(obj, metadata.Columns);
          parameters[metadata.PrimaryKey.Name] = metadata.PrimaryKey.GetValue(obj)!;
          
          var sql = $"UPDATE {metadata.TableName} SET {columnUpdates} WHERE [{metadata.PrimaryKey.Name}] = @{metadata.PrimaryKey.Name}";
          await ExecuteAsync(sql, parameters);
      }

      public async Task UpdateFieldAsync<T>(int id, string fieldName, object value) where T : class
      {
          ValidateFieldName<T>(fieldName);
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"UPDATE {metadata.TableName} SET [{fieldName}] = @value WHERE [{metadata.PrimaryKey.Name}] = @id";
          await ExecuteAsync(sql, new { id, value });
      }
      
      public async Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : class
      {
          var entityList = entities.ToList();
          if (entityList.Count == 0) return;

          var metadata = DbAttributes.GetTableMetadata<T>();

          entityList = entityList.Select(e =>
          {
              if (metadata.Columns.Any(c => c.Name == "CreatedAt"))
                  e = SetEntityProp(e, "CreatedAt", DateTime.UtcNow);

              var userId = _auditContext.UserId;
              if (userId != null && metadata.Columns.Any(c => c.Name == "CreatedBy"))
                  e = SetEntityProp(e, "CreatedBy", userId.Value);

              return e;
          }).ToList();

          var columnNames = string.Join(",", metadata.Columns.Select(c => $"[{c.Name}]"));
          var parameterNames = string.Join(",", metadata.Columns.Select(c => $"@{c.Name}"));

          var sql = $"INSERT INTO {metadata.TableName} ({columnNames}) VALUES ({parameterNames})";

          await ExecuteAsync(sql, entityList);
      }
      
      public async Task DeleteByIdAsync<T>(int id) where T : class
      {
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"DELETE FROM {metadata.TableName} WHERE [{metadata.PrimaryKey.Name}] = @id";
          await ExecuteAsync(sql, new { id });
      }

      public async Task DeleteByFieldAsync<T>(string fieldName, object value) where T : class
      {
          ValidateFieldName<T>(fieldName);
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"DELETE FROM {metadata.TableName} WHERE [{fieldName}] = @value";
          await ExecuteAsync(sql, new { value });
      }
      
      public async Task DeleteWhereInAsync<T>(string fieldName, List<int> values) where T : class
      {
          ValidateFieldName<T>(fieldName);
          if (values.Count == 0) return;
          
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"DELETE FROM {metadata.TableName} WHERE [{fieldName}] IN @values";
    
          await ExecuteAsync(sql, new { values });
      }

      // ITransactionManager Implementations
      
      public async Task<T> WithTransactionAsync<T>(Func<Task<T>> func)
      {
          // If a transaction is already active, reuse it to allow nested calls to have
          // the same transaction scope (supports propagation)
          if (_transaction != null) return await func();
          EnsureConnectionOpen();
          
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
      
      // Other
      
      private T SetEntityProp<T>(T obj, string field, object value) where T : class
      {
          var type = typeof(T);
          var prop = type.GetProperty(field);
          if (prop == null) return obj;

          prop.SetValue(obj, value);
          return obj;
      }

      private void ValidateFieldName<T>(string fieldName)
      {
          if (string.IsNullOrWhiteSpace(fieldName))
              throw new ArgumentException("Field name is required", nameof(fieldName));
          if (!DbAttributes.ValidateColumnExists<T>(fieldName))
              throw new ArgumentException("Field not found", nameof(fieldName));
      }

      private void EnsureConnectionOpen()
      {
          if (_db.State != ConnectionState.Open)
              _db.Open();
      }


      public IDbTransaction? Transaction => _transaction;
      
      public string Database => _db.Database;

      public async ValueTask DisposeAsync()
      {
          if (_disposed) return;

          if (_transaction != null)
              await _transaction.DisposeAsync();

          if (_db.State == ConnectionState.Open)
              await _db.CloseAsync();

          await _db.DisposeAsync();
          _disposed = true;
      }

      public void Dispose()
      {
          if (_disposed) return;

          _transaction?.Dispose();
          if (_db.State == ConnectionState.Open)
              _db.Close();
          
          _db.Dispose();
          _disposed = true;
      }
}