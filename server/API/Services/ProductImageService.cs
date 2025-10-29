using System.Net;
using API.Database;
using API.Extensions;
using API.Models;
using API.Models.DboTables;
using API.Models.Dtos;
using API.Services.Images;

namespace API.Services;

public interface IProductImageService
{
    Task<Result<ProductImageDto?>> GetPrimaryProductImageAsync(int productId);
    Task<Result<List<ProductImageDto>>> GetAllProductImagesAsync(int productId);
    Task<Result<List<ProductImageDto>>> GetPrimaryImagesForProductIds(List<int> productIds);
    Task<Result<ProductImageDto>> AddProductImageAsync(int productId, AddProductImageDto dto);
    Task<Result<bool>> SetPrimaryImageAsync(int productId, int productImageId);
    Task<Result<bool>> DeleteProductImageAsync(int productId, int productImageId);
    Task<Result<bool>> ReorderProductImagesAsync(int productId, List<int> orderedImageIds);
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
    

    public async Task<Result<ProductImageDto?>> GetPrimaryProductImageAsync(int productId)
    {
        var productExists = await _queryExecutor.ExistsAsync<Product>(productId);
        if (!productExists)
            return Result<ProductImageDto?>.Failure($"Product {productId} not found", HttpStatusCode.NotFound);

        var productImage = await _queryExecutor.FirstOrDefaultAsync<ProductImage>(
            """
            SELECT * FROM dbo.productImage 
            WHERE productId = @productId AND displayOrder = 0
            """, new { productId });
        return Result<ProductImageDto?>.Success(productImage != null ? MapToDto(productImage) : null);
    }

    public async Task<Result<List<ProductImageDto>>> GetAllProductImagesAsync(int productId)
    {
        var productExists = await _queryExecutor.ExistsAsync<Product>(productId);
        if (!productExists)
            return Result<List<ProductImageDto>>.Failure($"Product {productId} not found", HttpStatusCode.NotFound);

        var result = (await _queryExecutor.GetByFieldAsync<ProductImage>("productId", productId))
            .Select(MapToDto).OrderBy(pi => pi.DisplayOrder).ToList();
        return Result<List<ProductImageDto>>.Success(result);
    }

    public async Task<Result<List<ProductImageDto>>> GetPrimaryImagesForProductIds(List<int> productIds)
    {
        var result = (await _queryExecutor.GetWhereInAsync<ProductImage>("productId", productIds))
            .Where(img => img.DisplayOrder == 0)
            .Select(MapToDto)
            .OrderBy(pi => pi.ProductId)
            .ToList();
        return Result<List<ProductImageDto>>.Success(result);
    }

    public async Task<Result<ProductImageDto>> AddProductImageAsync(int productId, AddProductImageDto dto)
    {
        var validateProduct = await ValidateProduct(productId);
        if (!validateProduct.IsSuccess)
            return validateProduct.ToFailure<bool, ProductImageDto>();

        try
        {
            ProductImage productImage = null!;

            await _transactionManager.WithTransactionAsync(async () =>
            {
                var relativePath = await _imageStorageService.SaveImageAsync(dto.File, "products", productId.ToString());
                var imageCount = await _queryExecutor.GetCountByFieldAsync<ProductImage>("productId", productId);

                productImage = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = relativePath,
                    AltText = dto.AltText,
                    DisplayOrder = imageCount
                };

                var imageId = await _commandExecutor.InsertAsync(productImage);
                productImage.ProductImageId = imageId;

                // Move to display order = 0 and shift others
                if (dto.SetAsPrimary)
                {
                    await FixDisplayOrderAsync(productId, imageId);
                    productImage.DisplayOrder = 0;
                }
            });

            _logger.LogInformation("Product Image Added: ProductId {ProductId}, ImageId {ProductImageId}", 
                productId, productImage.ProductImageId);
            return Result<ProductImageDto>.Success(MapToDto(productImage));
        }
        catch (Exception ex)
        {
            return Result<ProductImageDto>.Failure($"Failed to add image: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    // Primary means displayOrder = 0
    public async Task<Result<bool>> SetPrimaryImageAsync(int productId, int productImageId)
    {
        var validateProduct = await ValidateProduct(productId);
        if (!validateProduct.IsSuccess)
            return validateProduct.ToFailure<bool, bool>();
        
        var image = await _queryExecutor.GetByIdAsync<ProductImage>(productImageId);
        if (image == null || image.ProductId != productId)
            return Result<bool>.Failure("Image not found", HttpStatusCode.NotFound);
        
        if (image.DisplayOrder == 0)
            return Result<bool>.Success(true);

        try
        {
            await _transactionManager.WithTransactionAsync(async () =>
            {
                await FixDisplayOrderAsync(image.ProductId, productImageId);
            });

            _logger.LogInformation("Product Image Set to Primary: ProductId {ProductId}, ImageId {ProductImageId}", 
                productId, productImageId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to set primary image: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeleteProductImageAsync(int productId, int productImageId)
    {
        var validateProduct = await ValidateProduct(productId);
        if (!validateProduct.IsSuccess)
            return validateProduct.ToFailure<bool, bool>();
        
        var image = await _queryExecutor.GetByIdAsync<ProductImage>(productImageId);
        if (image == null || image.ProductId != productId)
            return Result<bool>.Failure("Image not found", HttpStatusCode.NotFound);

        try
        {
            await _transactionManager.WithTransactionAsync(async () =>
            {
                await _commandExecutor.DeleteByIdAsync<ProductImage>(productImageId);

                var deleted = await _imageStorageService.DeleteImageAsync(image.ImageUrl);
                if (!deleted)
                    throw new Exception("Failed to delete image file");
                
                await FixDisplayOrderAsync(image.ProductId);
            });

            _logger.LogInformation("Product Image Deleted: ProductId {ProductId}, ImageId {ProductImageId}", 
                productId, productImageId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete image: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<bool>> ReorderProductImagesAsync(int productId, List<int> orderedImageIds)
    {
        var validateProduct = await ValidateProduct(productId);
        if (!validateProduct.IsSuccess)
            return validateProduct.ToFailure<bool, bool>();
        
        try
        {
            await _transactionManager.WithTransactionAsync(async () =>
            {
                var allImages = await _queryExecutor.GetByFieldAsync<ProductImage>("productId", productId);
                var imageDict = allImages.ToDictionary(i => i.ProductImageId);

                if (orderedImageIds.Any(imageId => !imageDict.ContainsKey(imageId)))
                    throw new Exception("Image does not belong to product");
                
                for (var i = 0; i < orderedImageIds.Count; i++)
                {
                    var image = imageDict[orderedImageIds[i]];
                    image.DisplayOrder = i;
                    await _commandExecutor.UpdateAsync(image);
                }
            });

            _logger.LogInformation("Product Images Re-ordered: ProductId {ProductId}", productId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to reorder images: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
    
    private async Task FixDisplayOrderAsync(int productId, int? newPrimaryImageId = null)
    {
        var images = (await _queryExecutor.GetByFieldAsync<ProductImage>("productId", productId)).OrderBy(i => i.DisplayOrder).ToList();

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
            await _commandExecutor.UpdateAsync(images[i]);
        }
    }

    private async Task<Result<bool>> ValidateProduct(int productId)
    {
        var productExists = await _queryExecutor.ExistsAsync<Product>(productId);
        if (!productExists)
            return Result<bool>.Failure($"Product {productId} not found", HttpStatusCode.NotFound);
        
        return await _productAuthService.CanUserManageProductAsync(productId);
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