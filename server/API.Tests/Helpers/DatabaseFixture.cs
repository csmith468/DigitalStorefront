namespace API.Tests.Helpers;

/// <summary>
/// xUnit fixture that manages database container lifecycle
/// Container starts once for all tests and is shared across all test classes to speed up test execution
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly TestDatabaseManager _testDatabaseManager;
    public CustomWebApplicationFactory Factory { get; private set; } = null!;

    public DatabaseFixture()
    {
        _testDatabaseManager = new TestDatabaseManager();
    }

    // Called once before any tests run (start container and run migrations)
    public async Task InitializeAsync()
    {
        await _testDatabaseManager.InitializeAsync();
        Factory = new CustomWebApplicationFactory(_testDatabaseManager);
    }

    // Called after tests complete (stop and remove container)
    public async Task DisposeAsync()
    {
        if (Factory != null)
            await Factory.DisposeAsync();
        await _testDatabaseManager.DisposeAsync();
    }
}