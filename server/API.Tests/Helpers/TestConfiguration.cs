using Microsoft.Extensions.Configuration;

namespace API.Tests.Helpers;

public static class TestConfiguration
{
    private static readonly IConfiguration _configuration;

    static TestConfiguration()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: false)
            .Build();
    }

    public static string SqlServerImage => GetRequiredValue("TestContainer:SqlServerImage");
    public static string SqlServerPassword => GetRequiredValue("TestContainer:SqlServerPassword");
    
    public static string TestAdminUsername => GetRequiredValue("TestAdmin:Username");
    public static string TestAdminPassword => GetRequiredValue("TestAdmin:Password");

    private static string GetRequiredValue(string key)
    {
        return _configuration[key] ?? throw new InvalidOperationException(
                   $"Required test configuration '{key}' is missing from appsettings.Test.json");
    }
}