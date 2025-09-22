using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace API.Database;

public interface IDataContextDapper : IDisposable
{
    List<T> Query<T>(string sql, object parameters = null);
    T First<T>(string sql, object parameters = null);
    T FirstOrDefault<T>(string sql, object parameters = null);
    
    int Execute(string sql, object parameters = null);
    int ExecuteStoredProcedure(string sql, object parameters = null);

    T WithTransaction<T>(Func<T> func);
    void WithTransaction(Action action);
    
    int Insert<T>(T obj) where T : class;
    void Update<T>(T obj) where T : class;
    T GetById<T>(int id) where T : class;
    List<T> GetWhere<T>(Dictionary<string, object> whereConditions) where T : class;
    List<T> GetByField<T>(string fieldName, object value) where T : class;
    bool Exists<T>(int id) where T : class;
    bool ExistsByField<T>(string fieldName, object value) where T : class;
    
    string Database { get; }
    IDbTransaction Transaction { get; }
}

public class DataContextDapper : IDataContextDapper
{
      private readonly IDbConnection _db;
      private IDbTransaction _transaction;

      public DataContextDapper(IConfiguration config)
      {
          _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
          _db.Open();
      }

      public List<T> Query<T>(string sql, object parameters = null)
      {
          return _db.Query<T>(sql, parameters, _transaction).ToList();
      }

      public T First<T>(string sql, object parameters = null)
      {
          return _db.QueryFirst<T>(sql, parameters, _transaction);
      }
      
      public T FirstOrDefault<T>(string sql, object parameters = null)
      {
          return _db.QueryFirstOrDefault<T>(sql, parameters, _transaction);
      }

      public int Execute(string sql, object parameters = null)
      {
          return _db.Execute(sql, parameters, _transaction);
      }

      public int ExecuteStoredProcedure(string sql, object parameters = null)
      {
          return _db.Execute(sql, parameters, _transaction, commandType: CommandType.StoredProcedure);
      }

      public T WithTransaction<T>(Func<T> func)
      {
          if (_transaction != null) return func();
          
          try
          {
              _transaction = _db.BeginTransaction();
              var result = func();
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

      public void WithTransaction(Action action)
      {
          WithTransaction<object>(() =>
          {
              action();
              return null;
          });
      }

      public int Insert<T>(T obj) where T : class
      {
          var tableName = DbAttributes.GetTableName(typeof(T));
          var columns = DbAttributes.GetDbColumnProperties(typeof(T));
          
          var columnNames = string.Join(",", columns.Select(c => $"[{c.Name}]"));
          var parameterNames = string.Join(",", columns.Select(c => $"@{c.Name}"));
          
          var parameters = DbAttributes.CreateParameters(obj, columns);
          var sql = $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameterNames}); SELECT CAST(SCOPE_IDENTITY() AS INT);";
          
          var result= Query<int>(sql, parameters).FirstOrDefault();
          return result == 0 ? throw new InvalidOperationException("Insert failed - no identity returned") : result;
      }

      public void Update<T>(T obj) where T : class
      {
          var tableName = DbAttributes.GetTableName(typeof(T));
          var columns = DbAttributes.GetDbColumnProperties(typeof(T));
          var primaryKey = DbAttributes.GetPrimaryKeyProperty(typeof(T));
          
          var columnUpdates = string.Join(",", columns.Select(c => $"[{c.Name}] = @{c.Name}"));
          var parameters = DbAttributes.CreateParameters(obj, columns);
          parameters[primaryKey.Name] = primaryKey.GetValue(obj);
          
          var sql = $"UPDATE {tableName} SET {columnUpdates} WHERE [{primaryKey.Name}] = @{primaryKey.Name}";
          Execute(sql, parameters);
      }

      public T GetById<T>(int id) where T : class
      {
          var tableName = DbAttributes.GetTableName(typeof(T));
          var primaryKey = DbAttributes.GetPrimaryKeyProperty(typeof(T));
          
          var sql = $"SELECT * FROM {tableName} WHERE [{primaryKey.Name}] = @id";
          return FirstOrDefault<T>(sql, new { id });
      }

      public List<T> GetWhere<T>(Dictionary<string, object> whereConditions) where T : class
      {
          if (whereConditions == null || !whereConditions.Any())
              throw new ArgumentException("At least one condition is required");
          
          var tableName = DbAttributes.GetTableName(typeof(T));
          var whereClause = string.Join(" AND ", whereConditions.Keys.Select(key => $"[{key}] = @{key}"));
          var sql = $"SELECT * FROM {tableName} WHERE {whereClause}";
          return Query<T>(sql, whereConditions);
      }

      public List<T> GetByField<T>(string fieldName, object value) where T : class
      {
          return GetWhere<T>(new Dictionary<string, object>{ { fieldName, value } });
      }

      public bool Exists<T>(int id) where T : class
      {
          return GetById<T>(id) != null;
      }

      public bool ExistsByField<T>(string fieldName, object value) where T : class
      {
          return GetWhere<T>(new Dictionary<string, object> { { fieldName, value } }).Count > 0;
      }

      public IDbTransaction Transaction => _transaction;
      
      public string Database => _db.Database;


      public void Dispose()
      {
          _transaction?.Dispose();
          _db?.Dispose();
      }
}