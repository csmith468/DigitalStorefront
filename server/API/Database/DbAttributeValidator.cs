using System.Reflection;

namespace API.Database;

public static class DbAttributeValidator
{
    public static void ValidateAllEntities(Assembly assembly)
    {
        var entityTypes = assembly.GetTypes().Where(t => t.GetCustomAttribute<DbTableAttribute>() != null).ToList();
        if (entityTypes.Count == 0)
        {
            throw new InvalidOperationException(
                "No entity types found with [DbTable] attribute. Ensure your entity models are decorated correctly.");
        }
        
        var errors = new List<string>();
        foreach (var entityType in entityTypes)
        {
            try
            {
                ValidateEntity(entityType);
            }
            catch (Exception ex)
            {
                errors.Add($"Invalid entity type name {entityType.Name}: {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            var errorMessage = "Entity validation failed:\n" + string.Join("\n", errors);
            throw new InvalidOperationException(errorMessage);
        }
        Console.WriteLine($"Validated {entityTypes.Count} entity types successfully!");
    }

    public static void ValidateSingleEntity(Type entityType)
    {
        ValidateEntity(entityType);
    }
    
    private static void ValidateEntity(Type entityType)
    {
        // Table Name
        var tableAttribute = entityType.GetCustomAttribute<DbTableAttribute>();
        if (tableAttribute == null)
            throw new InvalidOperationException("Missing [DbTable] attribute");
        if (string.IsNullOrWhiteSpace(tableAttribute.Name))
            throw new InvalidOperationException("[DbTable] name cannot be empty");

        // Primary Key
        var pkProperties = entityType.GetProperties().Where(p => p.GetCustomAttribute<DbPrimaryKeyAttribute>() != null).ToList();

        switch (pkProperties.Count)
        {
            case 0:
                throw new InvalidOperationException("Missing [DbPrimaryKey] attribute on any property");
            case > 1:
                throw new InvalidOperationException(
                    $"Multiple [DbPrimaryKey] attributes found on properties: {string.Join(", ", pkProperties.Select(p => p.Name))}");
        }

        var pkProperty = pkProperties[0];
        if (!pkProperty.CanRead || !pkProperty.CanWrite)
            throw new InvalidOperationException($"Primary key property {pkProperty.Name} must have getter and setter");

        // Columns
        var columnProperties = entityType.GetProperties().Where(p => p.GetCustomAttribute<DbColumnAttribute>() != null).ToList();

        if (columnProperties.Count == 0)
            throw new InvalidOperationException("No properties with [DbColumn] attribute found");
        foreach (var prop in columnProperties.Where(prop => !prop.CanRead || !prop.CanWrite))
            throw new InvalidOperationException($"Column property {prop.Name} must have getter and setter");
    }
}