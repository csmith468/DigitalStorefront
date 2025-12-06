using API.Services;

namespace API.Infrastructure.Startup;

public interface ICacheWarmer
{
    Task WarmCacheAsync(CancellationToken ct);
}

public class CacheWarmer : ICacheWarmer
{
    private readonly IMetadataService _metadataService;
    private readonly ILogger<CacheWarmer> _logger;

    public CacheWarmer(IMetadataService metadataService, ILogger<CacheWarmer> logger)
    {
        _metadataService = metadataService;
        _logger = logger;
    }
    
    public async Task WarmCacheAsync(CancellationToken ct)
    {
        var categories = await _metadataService.GetCategoriesAndSubcategoriesAsync(ct);
        var productTypes = await _metadataService.GetProductTypesAsync(ct);
        var priceTypes = _metadataService.GetPriceTypes();
        
        _logger.LogInformation("Cache warmed. Categories: {Categories}, Product Types: {ProductTypes}, Price Types: {PriceTypes}",
            categories.Data?.Count ?? 0, productTypes.Data?.Count ?? 0, priceTypes.Count);
    }
}