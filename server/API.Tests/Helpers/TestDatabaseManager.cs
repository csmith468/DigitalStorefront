using Testcontainers.MsSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DatabaseManagement.Helpers;
using DatabaseManagement.Modes;
using DatabaseManagement.UserInteraction;

namespace API.Tests.Helpers;

/// <summary>
/// Manages SQL Server container lifecycle for integration tests
/// Uses the same DatabaseMigrator infrastructure as production for consistency
/// </summary>
public class TestDatabaseManager : IAsyncDisposable
{
    private readonly MsSqlContainer _container;
    private readonly IConfiguration _testConfig;
    private readonly IUserInteraction _userInteraction;
    private bool _isInitialized;

    public TestDatabaseManager()
    {
        _container = new MsSqlBuilder()
            .WithImage(TestConfiguration.SqlServerImage)
            .WithPassword(TestConfiguration.SqlServerPassword)
            .WithCleanUp(true)
            .Build();

        _testConfig = CreateApiConfiguration();

        _userInteraction = new AutoUserInteraction(
            TestConfiguration.TestAdminUsername,
            TestConfiguration.TestAdminPassword
        );
    }

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;
        
        Console.WriteLine("Starting SQL Server test container...");
        await _container.StartAsync();

        Console.WriteLine("Running initial database setup...");
        await ResetDatabaseAsync();

        _isInitialized = true;
        Console.WriteLine("Test database initialization complete!");
    }

    public async Task ResetDatabaseAsync()
    {
        Console.WriteLine("Resetting database...");

        // Uses AutoUserInteraction so prompts are automated
        var resetMode = new ResetMode(
            serviceProvider: new ServiceCollection().BuildServiceProvider(),
            connectionString: ConnectionString,
            config: _testConfig,
            userInteraction: _userInteraction
        );

        await resetMode.ExecuteAsync();

        Console.WriteLine("Database reset complete!");
    }

    public async ValueTask DisposeAsync()
    {
        Console.WriteLine("Stopping SQL Server test container...");
        await _container.DisposeAsync();
    }
    
    private static IConfiguration CreateApiConfiguration()
    {
        var apiPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "API");

        return new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
    }
}