using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace API.Tests.Helpers;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly TestDatabaseManager _databaseManager;

    public CustomWebApplicationFactory(TestDatabaseManager databaseManager)
    {
        _databaseManager = databaseManager;
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override connection string to point to test container
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