namespace API.Tests.Helpers;

/// <summary>
/// Base class for integration tests that need database access
/// Manages lifecycle of database and ensures database is reset between tests
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(DatabaseFixture fixture)
    {
        Factory = fixture.Factory;
        Client = Factory.CreateClient();
    }

    // Called before each test to reset database
    public async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    // Called after database to cleanup if needed
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    // Prevents rate limit state from being shared across tests running in parallel
    protected void IsolateRateLimitingPerTest()
    {
        Client.DefaultRequestHeaders.Add("Test-Partition-Key", Guid.NewGuid().ToString());
    }
}