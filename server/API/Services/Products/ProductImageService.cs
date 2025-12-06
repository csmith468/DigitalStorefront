using API.Database;
using API.Extensions;
using API.Models;
using API.Models.DboTables;
using API.Models.Dtos;
using API.Services.Images;

namespace API.Services.Products;

public interface IProductImageService
{
    Task<Result<ProductImageDto?>> GetPrimaryProductImageAsync(int productId, CancellationToken ct = default);
    Task<Result<List<ProductImageDto>>> GetAllProductImagesAsync(int productId, CancellationToken ct = default);
    Task<Result<List<ProductImageDto>>> GetPrimaryImagesForProductIdsAsync(List<int> productIds, CancellationToken ct = default);
    Task<Result<ProductImageDto>> AddProductImageAsync(int productId, AddProductImageDto dto, CancellationToken ct = default);
    Task<Result<bool>> SetPrimaryImageAsync(int productId, int productImageId, CancellationToken ct = default);
    Task<Result<bool>> DeleteProductImageAsync(int productId, int productImageId, CancellationToken ct = default);
    Task<Result<bool>> ReorderProductImagesAsync(int productId, List<int> orderedImageIds, CancellationToken ct = default);
}

public class ProductImageService : IProductImageService
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<ProductImageService> _logger;
    private readonly IImageStorageService _imageStorageService;
    private readonly IProductAuthorizationService _productAuthService;

    public ProductImageService(
        IQueryExecutor queryExecutor,
        ICommandExecutor commandExecutor,
        ITransactionManager transactionManager,
        ILogger<ProductImageService> logger,
        IImageStorageService imageStorageService,
        IProductAuthorizationService productAuthService)
    {
        _queryExecutor = queryExecutor;
        _commandExecutor = commandExecutor;
        _transactionManager = transactionManager;
        _logger = logger;
        _imageStorageService = imageStorageService;
        _productAuthService = productAuthService;
    }
    

    public async Task<Result<ProductImageDto?>> GetPrimaryProductImageAsync(int productId, CancellationToken ct = default)
    {
        var productExists = await _queryExecutor.ExistsAsync<Product>(productId, ct);
        if (!productExists)
            return Result<ProductImageDto?>.Failure(ErrorMessages.Product.NotFound(productId));

        var productImage = await _queryExecutor.FirstOrDefaultAsync<ProductImage>(
            """
            SELECT * FROM dbo.productImage 
            WHERE productId = @productId AND displayOrder = 0
            """, new { productId }, ct);
        return Result<ProductImageDto?>.Success(productImage != null ? MapToDto(productImage) : null);
    }

    public async Task<Result<List<ProductImageDto>>> GetAllProductImagesAsync(int productId, CancellationToken ct = default)
    {
        var productExists = await _queryExecutor.ExistsAsync<Product>(productId, ct);
        if (!productExists)
            return Result<List<ProductImageDto>>.Failure(ErrorMessages.Product.NotFound(productId));

        var result = (await _queryExecutor.GetByFieldAsync<ProductImage>("productId", productId, ct))
            .Select(MapToDto).OrderBy(pi => pi.DisplayOrder).ToList();
        return Result<List<ProductImageDto>>.Success(result);
    }

    public async Task<Result<List<ProductImageDto>>> GetPrimaryImagesForProductIdsAsync(List<int> productIds, CancellationToken ct = default)
    {
        var result = (await _queryExecutor.GetWhereInAsync<ProductImage>("productId", productIds, ct))
            .Where(img => img.DisplayOrder == 0)
            .Select(MapToDto)
            .OrderBy(pi => pi.ProductId)
            .ToList();
        return Result<List<ProductImageDto>>.Success(result);
    }

    public async Task<Result<ProductImageDto>> AddProductImageAsync(int productId, AddProductImageDto dto, CancellationToken ct = default)
    {
        var validateProduct = await ValidateProductAsync(productId, ct);
        if (!validateProduct.IsSuccess)
            return validateProduct.ToFailure<bool, ProductImageDto>();

        string? uploadedImageUrl = null;

        try
        {
            ProductImage productImage = null!;

            await _transactionManager.WithTransactionAsync(async () =>
            {
                uploadedImageUrl = await _imageStorageService.SaveImageAsync(dto.File, "products", productId.ToString(), ct);
                var imageCount = await _queryExecutor.GetCountByFieldAsync<ProductImage>("productId", productId, ct);

                productImage = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = uploadedImageUrl,
                    AltText = dto.AltText,
                    DisplayOrder = imageCount
                };

                var imageId = await _commandExecutor.InsertAsync(productImage, ct);
                productImage.ProductImageId = imageId;

                // Move to display order = 0 and shift others
                if (dto.SetAsPrimary)
                {
                    await FixDisplayOrderAsync(productId, imageId, ct);
                    productImage.DisplayOrder = 0;
                }
            }, ct);

            _logger.LogInformation("Product Image Added: ProductId {ProductId}, ImageId {ProductImageId}", 
                productId, productImage.ProductImageId);
            return Result<ProductImageDto>.Success(MapToDto(productImage));
        }
        catch (Exception ex)
        {
            if (uploadedImageUrl != null)
            {
                try
                {
                    await _imageStorageService.DeleteImageAsync(uploadedImageUrl, ct);
                    _logger.LogInformation("Cleaned up orphaned image {ImageUrl} after transaction failure",
                        uploadedImageUrl);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "Failed to clean up orphaned image {ImageUrl}", uploadedImageUrl);
                }
            }
            
            _logger.LogError(ex, "Failed to add image to product {ProductId}: {Message}", productId, ex.Message);
            return Result<ProductImageDto>.Failure(ErrorMessages.Image.AddFailed);
        }
    }

    // Primary means displayOrder = 0
    public async Task<Result<bool>> SetPrimaryImageAsync(int productId, int productImageId, CancellationToken ct = default)
    {
        var validateProduct = await ValidateProductAsync(productId, ct);
        if (!validateProduct.IsSuccess)
            return validateProduct.ToFailure<bool, bool>();
        
        var image = await _queryExecutor.GetByIdAsync<ProductImage>(productImageId, ct);
        if (image == null || image.ProductId != productId)
            return Result<bool>.Failure(ErrorMessages.Image.NotFound);
        
        if (image.DisplayOrder == 0)
            return Result<bool>.Success(true);

        try
        {
            await _transactionManager.WithTransactionAsync(async () =>
            {
                await FixDisplayOrderAsync(image.ProductId, productImageId, ct);
            }, ct);

            _logger.LogInformation("Product Image Set to Primary: ProductId {ProductId}, ImageId {ProductImageId}", 
                productId, productImageId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set primary image for product {ProductId}, image {ProductImageId}: {Message}",
                productId, productImageId, ex.Message);
            return Result<bool>.Failure(ErrorMessages.Image.SetPrimaryFailed);
        }
    }

    public async Task<Result<bool>> DeleteProductImageAsync(int productId, int productImageId, CancellationToken ct = default)
    {
        var validateProduct = await ValidateProductAsync(productId, ct);
        if (!validateProduct.IsSuccess)
            return validateProduct.ToFailure<bool, bool>();
        
        var image = await _queryExecutor.GetByIdAsync<ProductImage>(productImageId, ct);
        if (image == null || image.ProductId != productId)
            return Result<bool>.Failure(ErrorMessages.Image.NotFound);

        try
        {
            await _transactionManager.WithTransactionAsync(async () =>
            {
                await _commandExecutor.DeleteByIdAsync<ProductImage>(productImageId, ct);

                var deleted = await _imageStorageService.DeleteImageAsync(image.ImageUrl, ct);
                if (!deleted)
                    throw new Exception("Failed to delete image file");
                
                await FixDisplayOrderAsync(image.ProductId, null, ct);
            }, ct);

            _logger.LogInformation("Product Image Deleted: ProductId {ProductId}, ImageId {ProductImageId}", 
                productId, productImageId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image {ProductImageId} from product {ProductId}: {Message}",
                productImageId, productId, ex.Message);
            return Result<bool>.Failure(ErrorMessages.Image.DeleteFailed);
        }
    }

    public async Task<Result<bool>> ReorderProductImagesAsync(int productId, List<int> orderedImageIds, CancellationToken ct = default)
    {
        var validateProduct = await ValidateProductAsync(productId, ct);
        if (!validateProduct.IsSuccess)
            return validateProduct.ToFailure<bool, bool>();
        
        try
        {
            await _transactionManager.WithTransactionAsync(async () =>
            {
                var allImages = await _queryExecutor.GetByFieldAsync<ProductImage>("productId", productId, ct);
                var imageDict = allImages.ToDictionary(i => i.ProductImageId);

                if (orderedImageIds.Any(imageId => !imageDict.ContainsKey(imageId)))
                    throw new Exception("Image does not belong to product");
                
                for (var i = 0; i < orderedImageIds.Count; i++)
                {
                    var image = imageDict[orderedImageIds[i]];
                    image.DisplayOrder = i;
                    await _commandExecutor.UpdateAsync(image, ct);
                }
            }, ct);

            _logger.LogInformation("Product Images Re-ordered: ProductId {ProductId}", productId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reorder images for product {ProductId}: {Message}", productId, ex.Message);
            return Result<bool>.Failure(ErrorMessages.Image.ReorderFailed);
        }
    }
    
    private async Task FixDisplayOrderAsync(int productId, int? newPrimaryImageId = null, CancellationToken ct = default)
    {
        var images = (await _queryExecutor.GetByFieldAsync<ProductImage>("productId", productId, ct))
            .OrderBy(i => i.DisplayOrder).ToList();

        // If setting a new primary, move it to displayOrder = 0
        if (newPrimaryImageId.HasValue)
        {
            var newPrimaryImage = images.FirstOrDefault(i => i.ProductImageId == newPrimaryImageId.Value);

            if (newPrimaryImage != null)
            {
                // Remove new primary from previous position to 0 (shifts everything else down)
                images.Remove(newPrimaryImage);
                images.Insert(0, newPrimaryImage);
            }
        }

        for (var i = 0; i < images.Count; i++)
        {
            images[i].DisplayOrder = i;
            await _commandExecutor.UpdateAsync(images[i], ct);
        }
    }

    private async Task<Result<bool>> ValidateProductAsync(int productId, CancellationToken ct = default)
    {
        var productExists = await _queryExecutor.ExistsAsync<Product>(productId, ct);
        return !productExists
            ? Result<bool>.Failure(ErrorMessages.Product.NotFound(productId)) 
            : Result<bool>.Success(true);
    }

    private ProductImageDto MapToDto(ProductImage image)
    {
        return new ProductImageDto
        {
            ProductImageId = image.ProductImageId,
            ProductId = image.ProductId,
            ImageUrl = _imageStorageService.GetImageUrl(image.ImageUrl),
            AltText = image.AltText,
            IsPrimary = image.DisplayOrder == 0,
            DisplayOrder = image.DisplayOrder
        };
    }
}