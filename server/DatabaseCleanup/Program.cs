using API.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using API.Setup;
using API.Services.Images;
using DatabaseCleanup;

var builder = Host.CreateApplicationBuilder(args);

var apiConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "../API");
builder.Configuration
    .SetBasePath(apiConfigPath)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

builder.Services.AddAutoRegistration(typeof(SharedContainer).Assembly);
builder.Services.AddManualRegistrations();
builder.Services.AddMappings();

var host = builder.Build();

var container = host.Services.GetRequiredService<ISharedContainer>();
var imageService = host.Services.GetRequiredService<IImageStorageService>();
var dapper = container.Dapper;

var cleaner = new DatabaseCleaner(dapper, imageService);
if (cleaner.Confirm())
{
    await cleaner.ExecuteAsync();
    Console.WriteLine("\n✓ Cleanup complete!");
}
else Console.WriteLine("Cleanup cancelled.");

