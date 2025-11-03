using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace API.Tests.Helpers;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Override security keys for test environment (CI doesn't have appsettings.Development.json)
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:TokenKey"] = "ThisIsATestTokenKeyThatIsSufficientlyLongForValidation12345",
                ["AppSettings:PasswordKey"] = "ThisIsATestPasswordKeyThatIsSufficientlyLongForValidation12345"
            });
        });
    }
}