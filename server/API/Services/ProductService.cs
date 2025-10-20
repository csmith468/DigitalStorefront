using System.Net;
using API.Extensions;
using API.Models;
using API.Models.Constants;
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
    Task<Result<int>> CreateProductAsync(ProductFormDto dto, int userId);
    Task<Result<bool>> UpdateProductAsync(int productId, ProductFormDto dto, int userId);
    bool CanUserEditProduct(Product product, int userId);
}

public class ProductService(ISharedContainer container) : BaseService(container), IProductService
{
    private IProductImageService _productImageService => DepInj<IProductImageService>();
    
    public async Task<Result<ProductDetailDto>> GetProductByIdAsync(int productId)
    {
        var productResult = await GetOrFailAsync<Product>(productId);
        if (!productResult.IsSuccess)
            return productResult.ToFailure<Product, ProductDetailDto>();
        
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
            return validateSubcategory.ToFailure<bool, List<ProductDto>>();

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
            return validateCategory.ToFailure<bool, List<ProductDto>>();
        
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

    public async Task<Result<int>> CreateProductAsync(ProductFormDto dto, int userId)
    {
        if (!await CanUserCreateProduct(userId))
            return Result<int>.Failure("You do not have permission to create a product", HttpStatusCode.Unauthorized);
        
        if (await Dapper.ExistsByFieldAsync<Product>("slug", dto.Slug))
            return Result<int>.Failure($"Product slug {dto.Slug} already exists", HttpStatusCode.BadRequest);

        var validationResult = await ValidateProduct(dto);
        if (!validationResult.IsSuccess)
            return validationResult.ToFailure<bool, int>();

        var productId = 0;
        await Dapper.WithTransactionAsync(async () =>
        {
            var product = Mapper.Map<ProductFormDto, Product>(dto);
            productId = await Dapper.InsertAsync(product);
            
            var sku = GenerateSku(productId, product.Slug);
            await Dapper.ExecuteAsync(
                "UPDATE dbo.Product SET sku = @sku WHERE productId = @productId",
                new { sku, productId } 
            );

            await SetProductSubcategoriesAsync(productId, dto.SubcategoryIds);
        });

        if (productId == 0)
            return Result<int>.Failure("Product could not be created.", HttpStatusCode.InternalServerError);
        return Result<int>.Success(productId, HttpStatusCode.Created);
    }

    public async Task<Result<bool>> UpdateProductAsync(int productId, ProductFormDto dto, int userId)
    {
        var product = await Dapper.GetByIdAsync<Product>(productId);
        if (product == null)
            return Result<bool>.Failure("Product could not be found");
        if (!CanUserEditProduct(product, userId))
            return Result<bool>.Failure("You do not have permission to edit this product", HttpStatusCode.Unauthorized);
        if (product.Slug != dto.Slug && await Dapper.ExistsByFieldAsync<Product>("slug", dto.Slug))
            return Result<bool>.Failure($"Slug {dto.Slug} already exists", HttpStatusCode.BadRequest);

        var validationResult = await ValidateProduct(dto);
        if (!validationResult.IsSuccess)
            return validationResult.ToFailure<bool, bool>();

        await Dapper.WithTransactionAsync(async () =>
        {
            Mapper.Map(dto, product);
            await Dapper.UpdateAsync(product);

            await SetProductSubcategoriesAsync(productId, dto.SubcategoryIds);
        });
        return Result<bool>.Success(true, HttpStatusCode.NoContent);
    }
    
    public bool CanUserEditProduct(Product product, int userId)
    {
        if (!Config.GetValue<bool>("DemoMode")) return true;
        return product.CreatedBy == userId;
    }
    
    private async Task<bool> CanUserCreateProduct(int userId)
    {
        if (!Config.GetValue<bool>("DemoMode")) return true;
        var userProductCount = await Dapper.GetCountByFieldAsync<Product>("createdBy", userId);
        return userProductCount <= 3;
    }
    
    private async Task SetProductSubcategoriesAsync(int productId, List<int> updatedSubcategoriesIds)
    {
        var existingSubcategories = await Dapper.GetByFieldAsync<ProductSubcategory>("productId", productId);
        
        var subcategoryIdsToAdd = updatedSubcategoriesIds.Where(us => 
            !existingSubcategories.Select(es => es.SubcategoryId).Contains(us)).ToList();
        var subcategoriesToRemove = existingSubcategories.Where(es => 
            !updatedSubcategoriesIds.Contains(es.SubcategoryId)).ToList();
        
        foreach (var subcategoryId in subcategoryIdsToAdd)
            await Dapper.InsertAsync(new ProductSubcategory { ProductId = productId, SubcategoryId = subcategoryId });

        foreach (var subcategory in subcategoriesToRemove)
            await Dapper.DeleteByIdAsync<ProductSubcategory>(subcategory.ProductSubcategoryId);
    }

    private async Task<Result<List<ProductDto>>> ConvertProductsToProductDtos(List<Product> products)
    {
        var productIds = products.Select(p => p.ProductId).ToList();
        var primaryImages = await _productImageService.GetPrimaryImagesForProductIds(productIds);

        var productDtos = products.Select(p =>
        {
            var productDto = Mapper.Map<Product, ProductDto>(p);
            productDto.PrimaryImage = primaryImages.Data.FirstOrDefault(pi => pi.ProductId == p.ProductId);
            var priceType = PriceTypes.All.FirstOrDefault(pt => pt.PriceTypeId == p.PriceTypeId);
            productDto.PriceIcon = priceType != null ? priceType.Icon : "";
            return productDto;
        }).ToList();
        return Result<List<ProductDto>>.Success(productDtos);
    }

    private string GenerateSku(int productId, string slug)
    {
        return slug[..3].ToUpper() + "-" + productId.ToString("D5");
    }

    private async Task<Result<bool>> ValidateProduct(ProductFormDto dto)
    {
        try
        {
            var productTypeExists = await ValidateExistsAsync<ProductType>(dto.ProductTypeId);
            if (!productTypeExists.IsSuccess)
                return productTypeExists.ToFailure<bool, bool>();
            if (!PriceTypes.All.Select(pt => pt.PriceTypeId).Contains(dto.PriceTypeId))
                return Result<bool>.Failure("Price type could not be found", HttpStatusCode.NotFound);
            if (dto.Name == "")
                return Result<bool>.Failure("Product name cannot be empty", HttpStatusCode.BadRequest);
            if (dto.Slug == "")
                return Result<bool>.Failure("Product slug cannot be empty", HttpStatusCode.BadRequest);
            if (dto.PremiumPrice >= dto.Price)
                return Result<bool>.Failure("Premium price cannot be greater than regular price",
                    HttpStatusCode.BadRequest);
            if (dto.SubcategoryIds.Count == 0)
                return Result<bool>.Failure("Product must have at least one subcategory", HttpStatusCode.BadRequest);
            if (dto.Price <= 0 || dto.PremiumPrice <= 0)
                return Result<bool>.Failure("Prices must be greater than zero", HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message, HttpStatusCode.InternalServerError);
        }
        return Result<bool>.Success(true);
    }
}