using System.Reflection;

namespace API.Database;

public class DbTableAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}

public class DbPrimaryKeyAttribute : Attribute { }

public class DbColumnAttribute : Attribute { }

public static class DbAttributes
{
    public static Dictionary<string, object?> CreateParameters(object obj, List<PropertyInfo> properties)
    {
        return properties.ToDictionary(prop => prop.Name, prop => prop.GetValue(obj));
    }
    
    public static TableMetadata GetTableMetadata<T>()
    {
        ValidateTable<T>();
        
        var tableName = GetTableName(typeof(T));
        var columns = GetDbColumnProperties(typeof(T));
        var primaryKey = GetPrimaryKeyProperty(typeof(T));

        return new TableMetadata
        {
            TableName = tableName!,
            Columns = columns,
            PrimaryKey = primaryKey!
        };
    }

    public static bool ValidateColumnExists<T>(string fieldName)
    {
        var metadata = GetTableMetadata<T>();
        return string.Equals(metadata.PrimaryKey.Name, fieldName, StringComparison.CurrentCultureIgnoreCase) 
                    || metadata.Columns.Any(c => string.Equals(c.Name, fieldName, StringComparison.CurrentCultureIgnoreCase));
    }

    private static void ValidateTable<T>()
    {
        var tableName = GetTableName(typeof(T));
        var columns = GetDbColumnProperties(typeof(T));
        var primaryKey = GetPrimaryKeyProperty(typeof(T));
        if (tableName is null || columns.Count == 0 || primaryKey is null)
            throw new InvalidOperationException("Invalid table.");
    }
    
    private static string? GetTableName(Type type)
    {
        var tableAttribute = type.GetCustomAttribute<DbTableAttribute>();
        return tableAttribute?.Name;
    }

    private static PropertyInfo? GetPrimaryKeyProperty(Type type)
    {
        return type.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<DbPrimaryKeyAttribute>() != null);
    }

    private static List<PropertyInfo> GetDbColumnProperties(Type type)
    {
        return type.GetProperties().Where(p => p.GetCustomAttribute<DbColumnAttribute>() != null).ToList();
    }

    public record TableMetadata
    {
        public string TableName { get; init; } = "";
        public required PropertyInfo PrimaryKey { get; init; }
        public List<PropertyInfo> Columns { get; init; } = [];
    }
}

