using System.Net;
using API.Database;
using API.Models;
using API.Models.DboTables;

namespace API.Services;

public interface IProductAuthorizationService
{
    Task<Result<bool>> CanUserManageProductAsync(int productId);
    Result<bool> CanUserManageProduct(Product product);
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
    
    public async Task<Result<bool>> CanUserManageProductAsync(int productId)
    {
        var product = await _queryExecutor.GetByIdAsync<Product>(productId);
        if (product == null)
            return Result<bool>.Failure("Product not found", HttpStatusCode.NotFound);
        
        return CanUserManageProduct(product);
    }
    
    public Result<bool> CanUserManageProduct(Product product)
    {
        if (product.IsDemoProduct && _config.GetValue<bool>("DemoMode") && !_userContext.IsAdmin())
        {
            _logger.LogWarning("Demo product access denied: ProductId {ProductId}, IsDemoProduct {IsDemoProduct}, UserId {UserId}",
                product.ProductId, product.IsDemoProduct, _userContext.UserId);
            return Result<bool>.Failure("Demo products can only be managed by administrators", HttpStatusCode.Forbidden);
        }
        return Result<bool>.Success(true);

    }
}