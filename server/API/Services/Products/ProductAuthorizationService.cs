using API.Database;
using API.Infrastructure.Contexts;
using API.Models;
using API.Models.DboTables;

namespace API.Services.Products;

public interface IProductAuthorizationService
{
    Task<Result<bool>> CanUserManageProductAsync(int productId, CancellationToken ct = default);
    Result<bool> CanUserManageProduct(Product product);
    Task<bool> CanUserCreateProductAsync(int userId, CancellationToken ct = default);
}

public class ProductAuthorizationService : IProductAuthorizationService
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly ILogger<ProductAuthorizationService> _logger;
    private readonly IConfiguration _config;
    private readonly IUserContext _userContext;

    public ProductAuthorizationService(IQueryExecutor queryExecutor, 
        ILogger<ProductAuthorizationService> logger,
        IConfiguration config, 
        IUserContext userContext)
    {
        _queryExecutor = queryExecutor;
        _logger = logger;
        _config = config;
        _userContext = userContext;
    }
    
    public async Task<Result<bool>> CanUserManageProductAsync(int productId, CancellationToken ct = default)
    {
        var product = await _queryExecutor.GetByIdAsync<Product>(productId, ct);
        if (product == null)
            return Result<bool>.Failure(ErrorMessages.Product.NotFound(productId));
        
        return CanUserManageProduct(product);
    }
    
    public Result<bool> CanUserManageProduct(Product product)
    {
        if (product.IsDemoProduct && _config.GetValue<bool>("DemoMode") && !_userContext.IsAdmin())
        {
            _logger.LogWarning("Demo product access denied: ProductId {ProductId}, IsDemoProduct {IsDemoProduct}, UserId {UserId}",
                product.ProductId, product.IsDemoProduct, _userContext.UserId);
            return Result<bool>.Failure(ErrorMessages.Product.DemoProductRestricted);
        }
        return Result<bool>.Success(true);
    }
    
    public async Task<bool> CanUserCreateProductAsync(int userId, CancellationToken ct = default)
    {
        if (!_config.GetValue<bool>("DemoMode")) return true;
        var userProductCount = await _queryExecutor.GetCountByFieldAsync<Product>("createdBy", userId, ct);
        if (userProductCount <= 3) return true;

        _logger.LogWarning("Product creation limit reached: UserId {UserId}", _userContext.UserId);
        return false;
    }
}