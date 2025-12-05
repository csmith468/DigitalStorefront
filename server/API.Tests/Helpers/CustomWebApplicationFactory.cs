using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace API.Tests.Helpers;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly TestDatabaseManager _databaseManager;

    static CustomWebApplicationFactory()
    {
        // Set environment variables for configuration needed before ConfigureAppConfiguration runs
        // NOTE TO SELF: Uses __ (double underscore) for nested keys and __0, __1 for array indices
        Environment.SetEnvironmentVariable("Cors__DevOrigins__0", "http://localhost:5173");
        Environment.SetEnvironmentVariable("Cors__ProdOrigins__0", "http://localhost:5173");
        Environment.SetEnvironmentVariable("AppSettings__TokenKey", "test-token-key-minimum-32-characters-long");
        Environment.SetEnvironmentVariable("AppSettings__PasswordKey", "test-password-key-minimum-32-chars-long");
        Environment.SetEnvironmentVariable("DemoMode", "true");
        Environment.SetEnvironmentVariable("UseAzureStorage", "false");
        Environment.SetEnvironmentVariable("RateLimiting__Anonymous__PermitLimit", "30");
        Environment.SetEnvironmentVariable("RateLimiting__Auth__PermitLimit", "5");
        Environment.SetEnvironmentVariable("RateLimiting__Global__PermitLimit", "150");
        Environment.SetEnvironmentVariable("RateLimiting__Authenticated__TokenCapacity", "100");
        Environment.SetEnvironmentVariable("Caching__StaticDataExpirationDays", "1");
    }

    public CustomWebApplicationFactory(TestDatabaseManager databaseManager)
    {
        _databaseManager = databaseManager;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _databaseManager.ConnectionString
            });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _databaseManager.ResetDatabaseAsync();
    }
}