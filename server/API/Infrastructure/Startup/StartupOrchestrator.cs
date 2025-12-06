using Polly;
using Polly.Retry;

namespace API.Infrastructure.Startup;

public class StartupOrchestrator : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StartupOrchestrator> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public StartupOrchestrator(IServiceProvider serviceProvider, ILogger<StartupOrchestrator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (ex, delay, attempt, _) =>
                {
                    _logger.LogWarning(ex, "Startup task failed, retrying in {Delay}s (attempt {Attempt}/5)", delay.TotalSeconds, attempt);
                });
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting application initialization...");

        await _retryPolicy.ExecuteAsync(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            await SeedRolesAsync(scope, ct);
            await WarmCacheAsync(scope, ct);
        });

        _logger.LogInformation("Application initialization complete");
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private async Task SeedRolesAsync(IServiceScope scope, CancellationToken ct)
    {
        _logger.LogInformation("Seeding roles...");
        var seeder = scope.ServiceProvider.GetRequiredService<IRoleSeeder>();
        await seeder.EnsureRolesSeededAsync(ct);
    }

    private async Task WarmCacheAsync(IServiceScope scope, CancellationToken ct)
    {
        _logger.LogInformation("Warming cache...");
        var warmer = scope.ServiceProvider.GetRequiredService<ICacheWarmer>();
        await warmer.WarmCacheAsync(ct);
    }
}