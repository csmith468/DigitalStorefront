using DatabaseManagement;
using API.Database;
using API.Services;
using API.Services.Contexts;
using API.Services.Images;
using DatabaseManagement.Helpers;
using DatabaseManagement.UserInteraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var apiPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "API");
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

var configuration = new ConfigurationBuilder()
    .SetBasePath(apiPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .Build();

var connectionString = args.FirstOrDefault(a => a.Contains("Server="))
                       ?? configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);

services.AddLogging();
services.AddSingleton(typeof(ILogger), sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseManagement"));

services.AddScoped<IQueryExecutor, DataContextDapper>();
services.AddScoped<IAuditContext, SystemAuditContext>();
services.AddScoped<ImageCleaner>();
var apiWwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "API", "wwwroot");
services.AddSingleton<IStoragePathProvider>(new ConsoleStoragePathProvider(apiWwwRootPath));
services.AddScoped<IImageStorageService, LocalImageStorageService>();

var serviceProvider = services.BuildServiceProvider();
var userInteraction = new ConsoleUserInteraction();

var runner = new DatabaseManagementRunner(args, serviceProvider, connectionString, configuration, userInteraction);
var exitCode = await runner.RunAsync();

await serviceProvider.DisposeAsync();
return exitCode;