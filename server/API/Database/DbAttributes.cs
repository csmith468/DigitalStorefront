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
    public static string GetTableName(Type type)
    {
        var tableAttribute = type.GetCustomAttribute<DbTableAttribute>();
        return tableAttribute != null ? tableAttribute.Name : type.Name;
    }

    public static PropertyInfo GetPrimaryKeyProperty(Type type)
    {
        return type.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<DbPrimaryKeyAttribute>() != null);
    }

    public static List<PropertyInfo> GetDbColumnProperties(Type type)
    {
        return type.GetProperties().Where(p => p.GetCustomAttribute<DbColumnAttribute>() != null).ToList();
    }

    public static Dictionary<string, object> CreateParameters(object obj, List<PropertyInfo> properties)
    {
        return properties.ToDictionary(prop => prop.Name, prop => prop.GetValue(obj));
    }
}