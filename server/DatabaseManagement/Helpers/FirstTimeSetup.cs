using API.Configuration;
using API.Utils;
using Dapper;
using DatabaseManagement.UserInteraction;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DatabaseManagement.Helpers;

public class FirstTimeSetup
{
    private readonly string _connectionString;
    private readonly IConfiguration _config;
    private readonly IUserInteraction _userInteraction;

    public FirstTimeSetup(string connectionString, IConfiguration config, IUserInteraction userInteraction)
    {
        _connectionString = connectionString;
        _config = config;
        _userInteraction = userInteraction;
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.WriteLine($"First time setup detected!\n");

        var credentials = await _userInteraction.PromptForAdminCredentialsAsync();

        if (credentials == null)
        {
            _userInteraction.WriteLine("Setup cancelled.");
            return false;
        }

        var (username, password) = credentials.Value;
        
        Console.WriteLine("Creating your admin account...");
        var userId = await CreateAdminUserAsync(username, password);

        Console.WriteLine("Assigning seeded products to your account...");
        await AssignSeededProductsToUserAsync(userId);

        _userInteraction.WriteSuccess([
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
            @"INSERT INTO dsf.[user] (username, firstName, lastName, isActive, createdAt)
            VALUES (@username, 'Admin', 'User', 1, GETUTCDATE());
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