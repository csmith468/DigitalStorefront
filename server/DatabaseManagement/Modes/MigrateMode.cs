using DatabaseManagement.Helpers;
using DatabaseManagement.UserInteraction;
using Microsoft.Extensions.Configuration;

namespace DatabaseManagement.Modes;

public class MigrateMode
{
    private readonly string _connectionString;
    private readonly IConfiguration _config;
    private readonly IUserInteraction _userInteraction;

    public MigrateMode(string connectionString, IConfiguration config, IUserInteraction userInteraction)
    {
        _connectionString = connectionString;
        _config = config;
        _userInteraction = userInteraction;
    }

    public async Task<int> ExecuteAsync()
    {
        Console.WriteLine("MODE: Migrate (run new scripts only)\n");

        var migrator = new DatabaseMigrator(_connectionString);
        var firstTimeSetup = new FirstTimeSetup(_connectionString, _config, _userInteraction);
        var isFirstTimeSetup = await firstTimeSetup.IsFirstRunAsync();
        
        var success = await migrator.RunMigrationsAsync();
        if (!isFirstTimeSetup) return success ? 0 : -1;

        var setupSuccess = await firstTimeSetup.ExecuteAsync();
        return setupSuccess ? 0 : -1;
    }
}