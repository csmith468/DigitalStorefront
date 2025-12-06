namespace API.Services.Background;

public class CacheWarmingService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheWarmingService> _logger;

    public CacheWarmingService(IServiceProvider serviceProvider, ILogger<CacheWarmingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cache warming started");

        using var scope = _serviceProvider.CreateScope();
        var metadataService = scope.ServiceProvider.GetRequiredService<IMetadataService>();

        var categories = await metadataService.GetCategoriesAndSubcategoriesAsync(cancellationToken);
        var productTypes = await metadataService.GetProductTypesAsync(cancellationToken);
        var priceTypes = metadataService.GetPriceTypes();
        
        _logger.LogInformation("Cache warmed. Categories: {Categories}, Product Types: {ProductTypes}, Price Types: {PriceTypes}",
            categories.Data?.Count ?? 0, productTypes.Data?.Count ?? 0, priceTypes.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}