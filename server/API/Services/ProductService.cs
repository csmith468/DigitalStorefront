using API.Models;
using API.Models.DboTables;
using API.Models.Dtos;
using API.Setup;
using API.Utils;

namespace API.Services;

public interface IProductService
{
    Task<Result<ProductDetailDto>> GetProductByIdAsync(int productId);
    Task<Result<List<ProductDto>>> GetProductsBySubcategoryAsync(string subcategorySlug);
    Task<Result<List<ProductDto>>> GetProductsByCategoryAsync(string categorySlug);

}

public class ProductService(ISharedContainer container) : BaseService(container), IProductService
{
    private IProductImageService _productImageService => DepInj<IProductImageService>();
    
    public async Task<Result<ProductDetailDto>> GetProductByIdAsync(int productId)
    {
        var productResult = await GetOrFailAsync<Product>(productId);
        if (!productResult.IsSuccess)
            return Result<ProductDetailDto>.Failure(productResult.Error!);
        var detailDto = Mapper.Map<Product, ProductDetailDto>(productResult.Data);

        var imagesResult = await _productImageService.GetAllProductImagesAsync(productId);
        if (imagesResult.IsSuccess)
            detailDto.Images = imagesResult.Data;
        
        return Result<ProductDetailDto>.Success(detailDto);
    }

    public async Task<Result<List<ProductDto>>> GetProductsBySubcategoryAsync(string subcategorySlug)
    {
        var validateSubcategory = await ValidateExistsByFieldAsync<Subcategory>("slug", subcategorySlug);
        if (!validateSubcategory.IsSuccess)
            return Result<List<ProductDto>>.Failure(validateSubcategory.Error!, validateSubcategory.StatusCode);

        var products = (await Dapper.QueryAsync<Product>(
            """
                SELECT p.*
                FROM dbo.product p
                JOIN dbo.productSubcategory ps ON p.productId = ps.productId
                JOIN dbo.subcategory s ON s.subcategoryId = ps.subcategoryId
                WHERE s.slug = @subcategorySlug
            """,
            new { subcategorySlug })).ToList();

        return await ConvertProductsToProductDtos(products);
    }

    public async Task<Result<List<ProductDto>>> GetProductsByCategoryAsync(string categorySlug)
    {
        var validateCategory = await ValidateExistsByFieldAsync<Category>("slug", categorySlug);
        if (!validateCategory.IsSuccess)
            return Result<List<ProductDto>>.Failure(validateCategory.Error!, validateCategory.StatusCode);
        
        var products = (await Dapper.QueryAsync<Product>(
            """
                SELECT p.*
                FROM dbo.product p
                JOIN dbo.productSubcategory ps ON p.productId = ps.productId
                JOIN dbo.subcategory s ON s.subcategoryId = ps.subcategoryId
                JOIN dbo.category c ON c.categoryId = s.categoryId
                WHERE c.slug = @categorySlug
            """,
            new { categorySlug })).ToList();

        return await ConvertProductsToProductDtos(products);
    }

    private async Task<Result<List<ProductDto>>> ConvertProductsToProductDtos(List<Product> products)
    {
        var productIds = products.Select(p => p.ProductId).ToList();
        var primaryImages = await _productImageService.GetPrimaryImagesForProductIds(productIds);
        var priceTypes = await Dapper.GetAllAsync<PriceType>();

        var productDtos = products.Select(p =>
        {
            var productDto = Mapper.Map<Product, ProductDto>(p);
            productDto.PrimaryImage = primaryImages.Data.FirstOrDefault(pi => pi.ProductId == p.ProductId);
            var priceType = priceTypes.FirstOrDefault(pt => pt.PriceTypeId == p.PriceTypeId);
            productDto.PriceIcon = priceType != null ? priceType.Icon : "";
            return productDto;
        }).ToList();
        return Result<List<ProductDto>>.Success(productDtos);
    }
}