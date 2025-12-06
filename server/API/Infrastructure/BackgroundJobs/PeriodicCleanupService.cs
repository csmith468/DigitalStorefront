using API.Database;

namespace API.Infrastructure.BackgroundJobs;

// NOTE: Since only running one instance, this works as mini CRON job
public class PeriodicCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PeriodicCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6);

    public PeriodicCleanupService(IServiceProvider serviceProvider, ILogger<PeriodicCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for StartupOrchestrator to complete
        var startedTcs = new TaskCompletionSource();
        using var scope = _serviceProvider.CreateScope();
        var lifetime = scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(() => startedTcs.SetResult());
        await startedTcs.Task;

        // Wait for connections to stabilize after cold start
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        _logger.LogInformation("Periodic cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupUnusedTagsAsync(stoppingToken);
                await CleanupExpiredIdempotencyKeysAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during periodic cleanup");
            }

            await Task.Delay(_interval, stoppingToken);
        }
        _logger.LogInformation("Periodic cleanup service stopping");
    }

    private async Task CleanupUnusedTagsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var commandExecutor = scope.ServiceProvider.GetRequiredService<ICommandExecutor>();

        var deletedCount = await commandExecutor.ExecuteAsync(
            "DELETE FROM dbo.Tag WHERE tagId NOT IN (SELECT DISTINCT tagId FROM dbo.productTag)", null, ct);
        if (deletedCount > 0)
            _logger.LogInformation("Cleaned up {Count} unused tags", deletedCount);
    }

    private async Task CleanupExpiredIdempotencyKeysAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var commandExecutor = scope.ServiceProvider.GetRequiredService<ICommandExecutor>();

        var deletedCount = await commandExecutor.ExecuteAsync(
            "DELETE FROM dbo.idempotencyKey WHERE expiresAt < GETUTCDATE()", null, ct);
        if (deletedCount > 0)
            _logger.LogInformation("Cleaned up {Count} expired idempotency keys", deletedCount);
            
    }
}