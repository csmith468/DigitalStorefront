using DbUp;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace DatabaseManagement.Helpers;

public class DatabaseMigrator
{
    private readonly string _connectionString;

    public DatabaseMigrator(string connectionString)
    {
      _connectionString = connectionString;
    }

    public async Task<bool> RunMigrationsAsync()
    {
        return await Task.Run(() =>
        {
            var upgrader = DeployChanges.To
                .SqlDatabase(_connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .WithTransaction()
                .LogToConsole()
                .Build();

            var scriptsToExecute = upgrader.GetScriptsToExecute();

            if (scriptsToExecute.Count != 0)
            {
                Console.WriteLine("\nScripts to execute:");
                foreach (var script in scriptsToExecute)
                    Console.WriteLine($"   {script.Name}");
            }
            else
            {
                Console.WriteLine("\nNo new scripts to execute. Database is up to date.");
                return true;
            }

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nMigration failed: {result.Error}");
                Console.ResetColor();
                return false;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Migrations complete!");
            Console.ResetColor();
            return true;
        });
    }

    public async Task DropAllTablesAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var dropScript = @"
            -- Drop all foreign key constraints
            DECLARE @sql NVARCHAR(MAX) = '';

            SELECT @sql += 'ALTER TABLE [' + OBJECT_SCHEMA_NAME(parent_object_id) + '].[' + 
            OBJECT_NAME(parent_object_id) + '] DROP CONSTRAINT [' + name + '];'
            FROM sys.foreign_keys;

            EXEC sp_executesql @sql;

            -- Drop all tables
            SET @sql = '';

            SELECT @sql += 'DROP TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + '];'
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE';

            EXEC sp_executesql @sql;

            -- Drop DbUp tracking table if it exists
            IF OBJECT_ID('SchemaVersions', 'U') IS NOT NULL
            DROP TABLE SchemaVersions;
        ";

        await using var command = new SqlCommand(dropScript, connection);
        await command.ExecuteNonQueryAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("All tables dropped");
        Console.ResetColor();
    }
}