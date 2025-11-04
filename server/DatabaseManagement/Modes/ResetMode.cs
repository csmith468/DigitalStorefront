using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DatabaseManagement.Helpers;
using DatabaseManagement.UserInteraction;

namespace DatabaseManagement.Modes;

public class ResetMode
{
    private readonly ServiceProvider _serviceProvider;
    private readonly string _connectionString;
    private readonly IConfiguration _config;
    private readonly IUserInteraction _userInteraction;

    public ResetMode(ServiceProvider serviceProvider, string connectionString, IConfiguration config,
        IUserInteraction userInteraction)
{
        _serviceProvider = serviceProvider;
        _connectionString = connectionString;
        _config = config;
        _userInteraction = userInteraction;
    }

    public async Task<int> ExecuteAsync()
    {
        Console.WriteLine("MODE: Reset (delete data + files, drop all tables, rebuild from scripts)");
        _userInteraction.WriteWarning([
            "WARNING: This will",
            "   1. Delete all image files",
            "   2. Drop all database tables",
            "   3. Rebuild database from migration scripts",
            "   This action cannot be undone!"
        ]);
        
        var confirmed = await _userInteraction.ConfirmAsync("\nType 'yes' to continue");
        if (!confirmed)
        {
            _userInteraction.WriteLine("\nReset Cancelled");
            return 0;
        }
        
        var firstTimeSetup = new FirstTimeSetup(_connectionString, _config, _userInteraction);
        var isFirstTimeSetup = await firstTimeSetup.IsFirstRunAsync();

        // If first time setup, tables and images do not exist so skip and migrate only
        if (!isFirstTimeSetup)
        {
            if (HasImageCleanerService())
                await DeleteImagesAsync();
            else Console.WriteLine("\n=== Phase 1: Skipping Image Deletion (test mode) ===");
            
            await DropAllTablesAsync();
        }
        else Console.WriteLine("\nNo tables/images exist so skipping to migration");
        
        var success = await RebuildDatabaseAsync();
        
        if (success)
            _userInteraction.WriteSuccess(["Reset complete!"]);
        
        return 0;
    }

    private async Task DeleteImagesAsync()
    {
        Console.WriteLine("\n=== Phase 1: Deleting Images ===");

        var cleaner = _serviceProvider.GetRequiredService<ImageCleaner>();
        await cleaner.DeleteImagesOnlyAsync();
    }

    private async Task DropAllTablesAsync()
    {
        Console.WriteLine("\n=== Phase 2: Dropping Tables ===");

        var migrator = new DatabaseMigrator(_connectionString);
        await migrator.DropAllTablesAsync();
    }

    private async Task<bool> RebuildDatabaseAsync()
    {
        Console.WriteLine("\n=== Phase 3: Running Migrations ===");
        
        var firstTimeSetup = new FirstTimeSetup(_connectionString, _config, _userInteraction);
        var migrator = new DatabaseMigrator(_connectionString);
        
        var result = await migrator.RunMigrationsAsync();
        if (!result) return false;

        return await firstTimeSetup.ExecuteAsync();
    }
    
    private bool HasImageCleanerService()
    {
        try
        {
            return _serviceProvider.GetService<ImageCleaner>() != null;
        }
        catch
        {
            return false;
        }
    }
}