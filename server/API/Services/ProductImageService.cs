using System.Net;
using API.Extensions;
using API.Models;
using API.Models.DboTables;
using API.Models.Dtos;
using API.Services.Images;
using API.Setup;

namespace API.Services;

public interface IProductImageService
{
    Task<Result<ProductImageDto?>> GetPrimaryProductImageAsync(int productId);
    Task<Result<List<ProductImageDto>>> GetAllProductImagesAsync(int productId);
    Task<Result<List<ProductImageDto>>> GetPrimaryImagesForProductIds(List<int> productIds);
    Task<Result<ProductImageDto>> AddProductImageAsync(int productId, AddProductImageDto dto);
    Task<Result<bool>> SetPrimaryImageAsync(int productId, int productImageId);
    Task<Result<bool>> DeleteProductImageAsync(int productId, int productImageId);
}

public class ProductImageService(ISharedContainer container) : BaseService(container), IProductImageService
{
    private IImageStorageService _imageStorageService => DepInj<IImageStorageService>();

    public async Task<Result<ProductImageDto?>> GetPrimaryProductImageAsync(int productId)
    {
        var validateProduct = await ValidateExistsAsync<Product>(productId);
        if (!validateProduct.IsSuccess)
            return validateProduct.ToFailure<bool, ProductImageDto?>();

        var productImage = await Dapper.FirstOrDefaultAsync<ProductImage>(
            """
            SELECT * FROM dbo.productImage 
            WHERE productId = @productId AND displayOrder = 0
            """, new { productId });
        return Result<ProductImageDto?>.Success(productImage != null ? MapToDto(productImage) : null);
    }

    public async Task<Result<List<ProductImageDto>>> GetAllProductImagesAsync(int productId)
    {
        var validateProduct = await ValidateExistsAsync<Product>(productId);
        if (!validateProduct.IsSuccess)
            return validateProduct.ToFailure<bool, List<ProductImageDto>>();

        var result = (await Dapper.GetByFieldAsync<ProductImage>("productId", productId))
            .Select(MapToDto).OrderBy(pi => pi.DisplayOrder).ToList();
        return Result<List<ProductImageDto>>.Success(result);
    }

    public async Task<Result<List<ProductImageDto>>> GetPrimaryImagesForProductIds(List<int> productIds)
    {
        var result = (await Dapper.GetWhereInAsync<ProductImage>("productId", productIds))
            .Where(img => img.DisplayOrder == 0)
            .Select(MapToDto)
            .OrderBy(pi => pi.ProductId)
            .ToList();
        return Result<List<ProductImageDto>>.Success(result);
    }

    public async Task<Result<ProductImageDto>> AddProductImageAsync(int productId, AddProductImageDto dto)
    {
        var validateProduct = await ValidateExistsAsync<Product>(productId);
        if (!validateProduct.IsSuccess)
            return validateProduct.ToFailure<bool, ProductImageDto>();

        try
        {
            ProductImage productImage = null!;

            await Dapper.WithTransactionAsync(async () =>
            {
                var relativePath = await _imageStorageService.SaveImageAsync(dto.File, "products", productId.ToString());
                var imageCount = await Dapper.GetCountByFieldAsync<ProductImage>("productId", productId);

                productImage = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = relativePath,
                    AltText = dto.AltText,
                    DisplayOrder = imageCount
                };

                var imageId = await Dapper.InsertAsync(productImage);
                productImage.ProductImageId = imageId;

                // Move to display order = 0 and shift others
                if (dto.SetAsPrimary)
                {
                    await ReorderImagesAsync(productId, imageId);
                    productImage.DisplayOrder = 0;
                }
            });

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
        var image = await Dapper.GetByIdAsync<ProductImage>(productImageId);
        if (image == null || image.ProductId != productId)
            return Result<bool>.Failure("Image not found", HttpStatusCode.NotFound);

        if (image.DisplayOrder == 0)
            return Result<bool>.Success(true);

        try
        {
            await Dapper.WithTransactionAsync(async () =>
            {
                await ReorderImagesAsync(image.ProductId, productImageId);
            });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to set primary image: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeleteProductImageAsync(int productId, int productImageId)
    {
        var image = await Dapper.GetByIdAsync<ProductImage>(productImageId);
        if (image == null || image.ProductId != productId)
            return Result<bool>.Failure("Image not found", HttpStatusCode.NotFound);

        try
        {
            await Dapper.WithTransactionAsync(async () =>
            {
                await Dapper.DeleteByIdAsync<ProductImage>(productImageId);

                var deleted = await _imageStorageService.DeleteImageAsync(image.ImageUrl);
                if (!deleted)
                    throw new Exception("Failed to delete image file");
                
                await ReorderImagesAsync(image.ProductId);
            });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete image: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    private async Task ReorderImagesAsync(int productId, int? newPrimaryImageId = null)
    {
        var images = (await Dapper.GetByFieldAsync<ProductImage>("productId", productId)).OrderBy(i => i.DisplayOrder).ToList();

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
            await Dapper.UpdateAsync(images[i]);
        }
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