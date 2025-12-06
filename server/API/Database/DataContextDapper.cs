using System.Data;
using API.Models.Dtos;
using API.Services.Contexts;
using Microsoft.Data.SqlClient;
using Dapper;

namespace API.Database;

public class DataContextDapper : IQueryExecutor, ICommandExecutor, ITransactionManager, IDisposable, IAsyncDisposable
{
      private readonly string _connectionString;
      private SqlConnection? _db;
      private SqlTransaction? _transaction;
      private readonly IAuditContext _auditContext;
      private bool _disposed;

      // Create connections on demand
      private SqlConnection Db
      {
          get
          {
              EnsureConnectionOpen();
              return _db!;
          }
      }

      // NOTE: IAuditContext autopopulates CreatedBy/UpdatedBy fields on inserts/updates
      // This abstracts the user source (HTTP vs system user) so the data layer doesn't depend on ASP.NET
      // Keeps audit tracking DRY without pass userId everywhere or relying on people to remember to set fields
      
      public DataContextDapper(IConfiguration config, IAuditContext auditContext)
      {
          _connectionString = config.GetConnectionString("DefaultConnection")
                              ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
          _auditContext = auditContext;
      }

      // IQueryExecutor Implementations
      public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CancellationToken ct = default)
      {
          var command = CreateCommand(sql, parameters, ct: ct);
          return await Db.QueryAsync<T>(command);
      }

      public async Task<T> FirstAsync<T>(string sql, object? parameters = null, CancellationToken ct = default)
      {
          var command = CreateCommand(sql, parameters, ct: ct);
          return await Db.QueryFirstAsync<T>(command);
      }
      
      public async Task<T?> FirstOrDefaultAsync<T>(string sql, object? parameters = null, CancellationToken ct = default)
      {
          var command = CreateCommand(sql, parameters, ct: ct);
          return await Db.QueryFirstOrDefaultAsync<T>(command);
      }
      
      public async Task<IEnumerable<T>> GetAllAsync<T>(CancellationToken ct = default) where T : class
      {
          var metadata = DbAttributes.GetTableMetadata<T>();
          return await QueryAsync<T>($"SELECT * FROM {metadata.TableName}", null, ct);
      }

      public async Task<T?> GetByIdAsync<T>(int id, CancellationToken ct = default) where T : class
      {
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"SELECT * FROM {metadata.TableName} WHERE [{metadata.PrimaryKey.Name}] = @id";
          return await FirstOrDefaultAsync<T>(sql, new { id }, ct);
      }

      public async Task<IEnumerable<T>> GetWhereAsync<T>(Dictionary<string, object> whereConditions, CancellationToken ct = default) where T : class
      {
          if (whereConditions == null || whereConditions.Count == 0)
              throw new ArgumentException("At least one condition is required");

          foreach (var key in whereConditions.Keys.Where(key => !DbAttributes.ValidateColumnExists<T>(key)))
              throw new ArgumentException($"Field '{key}' not found", nameof(whereConditions));

          var metadata = DbAttributes.GetTableMetadata<T>();
          var whereClause = string.Join(" AND ", whereConditions.Keys.Select(key => $"[{key}] = @{key}"));
          var sql = $"SELECT * FROM {metadata.TableName} WHERE {whereClause}";
          return await QueryAsync<T>(sql, whereConditions, ct);
      }

      public async Task<IEnumerable<T>> GetByFieldAsync<T>(string fieldName, object value, CancellationToken ct = default) where T : class
      {
          ValidateFieldName<T>(fieldName);
          return await GetWhereAsync<T>(new Dictionary<string, object>{ { fieldName, value } }, ct);
      }

      public async Task<IEnumerable<T>> GetWhereInAsync<T>(string fieldName, List<int> values, CancellationToken ct = default) where T : class
      {
          ValidateFieldName<T>(fieldName);
          if (values.Count == 0) return [];
          
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"SELECT * FROM {metadata.TableName} WHERE [{fieldName}] IN @values";
    
          return await QueryAsync<T>(sql, new { values }, ct);
      }
      public async Task<IEnumerable<T>> GetWhereInStrAsync<T>(string fieldName, List<string> values, CancellationToken ct = default) where T : class
      {
          ValidateFieldName<T>(fieldName);
          if (values.Count == 0) return [];
          
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"SELECT * FROM {metadata.TableName} WHERE [{fieldName}] IN @values";
    
          return await QueryAsync<T>(sql, new { values }, ct);
      }

      public async Task<(IEnumerable<T> items, int totalCount)> GetPaginatedWithSqlAsync<T>(
          string baseQuery,
          PaginationParams paginationParams,
          object? parameters = null,
          string? orderByColumn = null,
          bool descending = true,
          TrustedOrderByExpression? customOrderBy = null,
          CancellationToken ct = default) where T : class
      {
          var countSql = $"SELECT COUNT(*) FROM ({baseQuery}) AS CountQuery";
          var countCommand = CreateCommand(countSql, parameters, ct: ct);
          var totalCount = await Db.ExecuteScalarAsync<int>(countCommand);

          string orderBy;

          if (customOrderBy != null)
          {
              // Order by must be marked as "Trusted" in function calling GetPaginatedWithSqlAsync,
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
          
          var paginatedCommand = CreateCommand(paginatedSql, allParms, ct: ct);
          var items = await Db.QueryAsync<T>(paginatedCommand);
          return (items, totalCount);
      }

      public async Task<bool> ExistsAsync<T>(int id, CancellationToken ct = default) where T : class
      {
          return await GetByIdAsync<T>(id, ct) != null;
      }

      public async Task<bool> ExistsByFieldAsync<T>(string fieldName, object value, CancellationToken ct = default) where T : class
      {
          if (!DbAttributes.ValidateColumnExists<T>(fieldName))
              throw new ArgumentException("Field not found", nameof(fieldName));
          return (await GetWhereAsync<T>(new Dictionary<string, object> { { fieldName, value } }, ct)).Any();
      }

      public async Task<int> GetCountByFieldAsync<T>(string fieldName, object value, CancellationToken ct = default) where T : class
      {
          if (!DbAttributes.ValidateColumnExists<T>(fieldName))
              throw new ArgumentException("Field not found", nameof(fieldName));
          return (await GetWhereAsync<T>(new Dictionary<string, object> { { fieldName, value } }, ct)).Count();
      }
      
      
      // ICommandExecutor Implementations
      public async Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken ct = default)
      {
          var command = CreateCommand(sql, parameters, ct: ct);
          return await Db.ExecuteAsync(command);
      }

      public async Task<int> ExecuteStoredProcedureAsync(string sql, object? parameters = null, CancellationToken ct = default)
      {
          var command = CreateCommand(sql, parameters, commandType: CommandType.StoredProcedure, ct: ct);
          return await Db.ExecuteAsync(command);
      }

      public async Task<int> InsertAsync<T>(T obj, CancellationToken ct = default) where T : class
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
          
          var result = (await QueryAsync<int>(sql, parameters, ct)).FirstOrDefault();
          return result == 0 ? throw new InvalidOperationException("Insert failed - no identity returned") : result;
      }

      public async Task UpdateAsync<T>(T obj, DateTime? expectedUpdatedAt, CancellationToken ct = default) where T : class
      {
          var metadata = DbAttributes.GetTableMetadata<T>();
          
          await VerifyConcurrencyAsync<T>((int)metadata.PrimaryKey.GetValue(obj)!, expectedUpdatedAt, ct);

          if (metadata.Columns.Any(c => c.Name == "UpdatedAt"))
              obj = SetEntityProp(obj, "UpdatedAt", DateTime.UtcNow);

          var userId = _auditContext.UserId;
          if (userId != null && metadata.Columns.Any(c => c.Name == "UpdatedBy"))
              obj = SetEntityProp(obj, "UpdatedBy", userId.Value);
          
          var columnUpdates = string.Join(",", metadata.Columns.Select(c => $"[{c.Name}] = @{c.Name}"));
          var parameters = DbAttributes.CreateParameters(obj, metadata.Columns);
          parameters[metadata.PrimaryKey.Name] = metadata.PrimaryKey.GetValue(obj)!;
          
          var sql = $"UPDATE {metadata.TableName} SET {columnUpdates} WHERE [{metadata.PrimaryKey.Name}] = @{metadata.PrimaryKey.Name}";
          await ExecuteAsync(sql, parameters, ct);
      }

      public async Task UpdateFieldAsync<T>(int id, string fieldName, object value, DateTime? expectedUpdatedAt, CancellationToken ct = default) where T : class
      {
          await VerifyConcurrencyAsync<T>(id, expectedUpdatedAt, ct);
          ValidateFieldName<T>(fieldName);
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"UPDATE {metadata.TableName} SET [{fieldName}] = @value WHERE [{metadata.PrimaryKey.Name}] = @id";
          await ExecuteAsync(sql, new { id, value }, ct);
      }
      
      public async Task BulkInsertAsync<T>(IEnumerable<T> entities, CancellationToken ct = default) where T : class
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

          await ExecuteAsync(sql, entityList, ct);
      }
      
      public async Task DeleteByIdAsync<T>(int id, CancellationToken ct = default) where T : class
      {
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"DELETE FROM {metadata.TableName} WHERE [{metadata.PrimaryKey.Name}] = @id";
          await ExecuteAsync(sql, new { id }, ct);
      }

      public async Task DeleteByFieldAsync<T>(string fieldName, object value, CancellationToken ct = default) where T : class
      {
          ValidateFieldName<T>(fieldName);
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"DELETE FROM {metadata.TableName} WHERE [{fieldName}] = @value";
          await ExecuteAsync(sql, new { value }, ct);
      }
      
      public async Task DeleteWhereInAsync<T>(string fieldName, List<int> values, CancellationToken ct = default) where T : class
      {
          ValidateFieldName<T>(fieldName);
          if (values.Count == 0) return;
          
          var metadata = DbAttributes.GetTableMetadata<T>();
          var sql = $"DELETE FROM {metadata.TableName} WHERE [{fieldName}] IN @values";
    
          await ExecuteAsync(sql, new { values }, ct);
      }

      public async Task VerifyConcurrencyAsync<T>(int id, DateTime? expectedUpdatedAt, CancellationToken ct = default) where T : class
      {
          var metadata = DbAttributes.GetTableMetadata<T>();
          if (metadata.Columns.All(c => c.Name != "UpdatedAt")) return;

          var sql = $"SELECT [UpdatedAt] FROM {metadata.TableName} WHERE [{metadata.PrimaryKey.Name}] = @id";
          var currentUpdatedAt = await FirstOrDefaultAsync<DateTime?>(sql, new { id }, ct);

          // Truncate to milliseconds to avoid precision issues from JSON serialization
          var currentTruncated = TruncateToMilliseconds(currentUpdatedAt);
          var expectedTruncated = TruncateToMilliseconds(expectedUpdatedAt);

          if (currentTruncated != expectedTruncated)
              throw new ConcurrencyException("Record was modified by another user. Please reload and try again.");
      }

      private static DateTime? TruncateToMilliseconds(DateTime? dt)
      {
          if (dt == null) return null;
          return new DateTime(dt.Value.Year, dt.Value.Month, dt.Value.Day,
              dt.Value.Hour, dt.Value.Minute, dt.Value.Second,
              dt.Value.Millisecond, dt.Value.Kind);
      }

      // ITransactionManager Implementations
      
      public async Task<T> WithTransactionAsync<T>(Func<Task<T>> func, CancellationToken ct = default)
      {
          // If a transaction is already active, reuse it to allow nested calls to have
          // the same transaction scope (supports propagation)
          if (_transaction != null) return await func();
          EnsureConnectionOpen();
          
          try
          {
              _transaction = Db.BeginTransaction();
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

      public async Task WithTransactionAsync(Func<Task> func, CancellationToken ct = default)
      {
          await WithTransactionAsync<object?>(async () =>
          {
              await func();
              return null;
          }, ct);
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

      private CommandDefinition CreateCommand(string sql, object? parameters = null,
          CommandType? commandType = null, CancellationToken ct = default)
      {
          return new CommandDefinition(sql, parameters, _transaction, commandType: commandType, cancellationToken: ct);
      }
      
      public IDbTransaction? Transaction => _transaction;
      
      public string Database => Db.Database;
      
      private void EnsureConnectionOpen()
      {
          if (_db is { State: ConnectionState.Open }) 
              return;
          
          _db?.Dispose();
          _db = new SqlConnection(_connectionString);
          _db.Open();
      }
      
      // NOTE: Async cleanup of resources for "await using" calls or explicitly calling await DisposeAsync
      public async ValueTask DisposeAsync()
      {
          if (_disposed) return;

          if (_transaction != null)
              await _transaction.DisposeAsync();

          if (_db != null)
          {
              if (_db.State == ConnectionState.Open)
                  await _db.CloseAsync();
              
              await _db.DisposeAsync();
          }
          
          _disposed = true;
          GC.SuppressFinalize(this); // already done manually, don't need to do it again through garbage collector
      }

      // NOTE: For synchronous cleanup without await (needed for backwards compatibility)
      public void Dispose()
      {
          if (_disposed) return;

          _transaction?.Dispose();

          if (_db != null)
          {
              if (_db.State == ConnectionState.Open)
                  _db.Close();
              
              _db.Dispose();
          }
          _disposed = true;
          GC.SuppressFinalize(this); // already done manually, don't need to do it again through garbage collector
      }
}