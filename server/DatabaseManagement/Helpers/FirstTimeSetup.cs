using API.Configuration;
using API.Utils;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DatabaseManagement.Helpers;

public class FirstTimeSetup
{
    private readonly string _connectionString;
    private readonly IConfiguration _config;

    public FirstTimeSetup(string connectionString, IConfiguration config)
    {
        _connectionString = connectionString;
        _config = config;
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.WriteLine($"First time setup detected!\n");
        Console.WriteLine("Let's create your admin account...");

        var (username, password) = PromptForAdminCredentials();

        if (username == null || password == null)
            return false;
        
        Console.WriteLine("Creating your admin account...");
        var userId = await CreateAdminUserAsync(username, password);

        Console.WriteLine("Assigning seeded products to your account...");
        await AssignSeededProductsToUserAsync(userId);

        Common.WriteGreenInConsole([
            $"Admin account created!",
            $"   Username: {username}",
            $"   You can now log in with these credentials."
        ]);

        return true;
    }

    public async Task<bool> IsFirstRunAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var tableExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchemaVersions'"
        );

        return tableExists == 0;
    }
    
    private (string?, string?) PromptForAdminCredentials()
    {
        Console.Write("Admin Username: ");
        var username = Console.ReadLine();

        string? password = null;
        while (password == null)
        {
            Console.Write("Admin Password: ");
            var passwordAttempt = ReadPassword();

            Console.Write("\nConfirm Password: ");
            var confirmPassword = ReadPassword();

            if (passwordAttempt != confirmPassword)
            {
                Console.WriteLine("\nPasswords don't match! Please try again.\n");
                continue;
            }

            password = passwordAttempt;
        }

        Console.WriteLine("\n");
        return (username, password);
    }

    private static string ReadPassword()
    {
        var password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(intercept: true); // Don't display the character

            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[0..^1];     // Remove last character
                Console.Write("\b \b");         // Erase the asterisk
            }
            else if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        } while (key.Key != ConsoleKey.Enter);

        return password;
    }

    private async Task<int> CreateAdminUserAsync(string username, string password)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();


        var passwordKey = _config["AppSettings:PasswordKey"]
                          ?? throw new InvalidOperationException("AppSettings:PasswordKey is missing from configuration");
        var securityOptions = new SecurityOptions { PasswordKey = passwordKey };
        var hasher = new PasswordHasher(Options.Create(securityOptions));

        var (passwordHash, passwordSalt) = hasher.HashPassword(password);

        var userId = await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO dsf.[user] (username, firstName, lastName, email, isActive, isAdmin, createdAt)
            VALUES (@username, 'Admin', 'User', @username, 1, 1, GETUTCDATE());
            SELECT CAST(SCOPE_IDENTITY() AS INT);",
            new { username }
        );

        await connection.ExecuteAsync(
            @"INSERT INTO dsf.auth (userId, passwordHash, passwordSalt)
            VALUES (@userId, @passwordHash, @passwordSalt)",
            new { userId, passwordHash, passwordSalt }
        );

        return userId;
    }

    private async Task AssignSeededProductsToUserAsync(int userId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            "UPDATE dbo.product SET createdBy = @userId, createdAt = GETUTCDATE() WHERE createdBy IS NULL",
            new { userId }
        );

        await connection.ExecuteAsync(
            "UPDATE dbo.productImage SET createdBy = @userId, createdAt = GETUTCDATE() WHERE createdBy IS NULL",
            new { userId }
        );
        
        await connection.ExecuteAsync(
            "UPDATE dbo.category SET createdBy = @userId, createdAt = GETUTCDATE() WHERE createdBy IS NULL",
            new { userId }
        );
        
        await connection.ExecuteAsync(
            "UPDATE dbo.subcategory SET createdBy = @userId, createdAt = GETUTCDATE() WHERE createdBy IS NULL",
            new { userId }
        );
    }
}