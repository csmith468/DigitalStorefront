
using DatabaseManagement.Modes;
using DatabaseManagement.UserInteraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseManagement;

public class DatabaseManagementRunner
{
    private readonly string[] _args;
    private readonly ServiceProvider _serviceProvider;
    private readonly string _connectionString;
    private readonly IConfiguration _config;
    private readonly IUserInteraction _userInteraction;

    public DatabaseManagementRunner(string[] args, ServiceProvider serviceProvider, string connectionString,
        IConfiguration config, IUserInteraction userInteraction)
    {
        _args = args;
        _serviceProvider = serviceProvider;
        _connectionString = connectionString;
        _config = config;
        _userInteraction = userInteraction;
    }

    public async Task<int> RunAsync()
    {
        Console.WriteLine("=== Digital Storefront Database Management ===\n");

        var mode = _args.FirstOrDefault() ?? "--help";

        return mode switch
        {
            "--migrate" => await new MigrateMode(_connectionString, _config, _userInteraction).ExecuteAsync(),
            "--reset" => await new ResetMode(_serviceProvider, _connectionString, _config, _userInteraction).ExecuteAsync(),
            "--help" or "--h" => ShowHelp(),
            _ => HandleUnknownMode(mode),
        };
    }

    private int HandleUnknownMode(string mode)
    {
        Console.WriteLine($"Unknown Mode: {mode}");
        ShowHelp();
        return -1;
    }
    
    private int ShowHelp()
    {
        Console.WriteLine("USAGE:");
        Console.WriteLine("  dotnet run [mode] [connection-string]");
        Console.WriteLine();
        Console.WriteLine("MODES:");
        Console.WriteLine("  --migrate   Run new migration scripts only");
        Console.WriteLine("  --reset     Drop all tables, delete images, rebuild from scripts");
        Console.WriteLine("  --help      Show this help message");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES:");
        Console.WriteLine("  dotnet run --clean");
        Console.WriteLine("  dotnet run --migrate");
        Console.WriteLine("  dotnet run --reset");
        Console.WriteLine("  dotnet run --migrate \"Server=myserver;Database=MyDb;...\"");
        return 0;
    }
}