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
    Task<Result<ProductDetailDto>> CreateProductAsync(ProductFormDto dto, int userId);
    Task<Result<ProductDetailDto>> UpdateProductAsync(int productId, ProductFormDto dto, int userId);
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
        
        var productDetailDto = await ConvertProductToProductDetailDto(productResult.Data);
        return Result<ProductDetailDto>.Success(productDetailDto);
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

    public async Task<Result<ProductDetailDto>> CreateProductAsync(ProductFormDto dto, int userId)
    {
        if (!await CanUserCreateProduct(userId))
            return Result<ProductDetailDto>.Failure("You do not have permission to create a product", HttpStatusCode.Unauthorized);
        
        if (await Dapper.ExistsByFieldAsync<Product>("slug", dto.Slug))
            return Result<ProductDetailDto>.Failure($"Product slug {dto.Slug} already exists", HttpStatusCode.BadRequest);

        var validationResult = await ValidateProduct(dto);
        if (!validationResult.IsSuccess)
            return validationResult.ToFailure<bool, ProductDetailDto>();

        var product = Mapper.Map<ProductFormDto, Product>(dto);
        await Dapper.WithTransactionAsync(async () =>
        {
            product.ProductId = await Dapper.InsertAsync(product);
            
            product.Sku = GenerateSku(product.ProductId, product.Slug);
            await Dapper.ExecuteAsync(
                "UPDATE dbo.Product SET sku = @sku WHERE productId = @productId",
                new { sku = product.Sku, product.ProductId } 
            );
            await SetProductSubcategoriesAsync(product.ProductId, dto.SubcategoryIds);
        });

        if (product.ProductId == 0)
            return Result<ProductDetailDto>.Failure("Product could not be created.", HttpStatusCode.InternalServerError);
        var productDetailDto = await ConvertProductToProductDetailDto(product);
        return Result<ProductDetailDto>.Success(productDetailDto, HttpStatusCode.Created);
    }

    public async Task<Result<ProductDetailDto>> UpdateProductAsync(int productId, ProductFormDto dto, int userId)
    {
        var product = await Dapper.GetByIdAsync<Product>(productId);
        if (product == null)
            return Result<ProductDetailDto>.Failure("Product could not be found");
        if (!CanUserEditProduct(product, userId))
            return Result<ProductDetailDto>.Failure("You do not have permission to edit this product", HttpStatusCode.Unauthorized);
        if (product.Slug != dto.Slug && await Dapper.ExistsByFieldAsync<Product>("slug", dto.Slug))
            return Result<ProductDetailDto>.Failure($"Slug {dto.Slug} already exists", HttpStatusCode.BadRequest);

        var validationResult = await ValidateProduct(dto);
        if (!validationResult.IsSuccess)
            return validationResult.ToFailure<bool, ProductDetailDto>();

        await Dapper.WithTransactionAsync(async () =>
        {
            Mapper.Map(dto, product);
            await Dapper.UpdateAsync(product);

            await SetProductSubcategoriesAsync(productId, dto.SubcategoryIds);
        });
        
        var productDetailDto = await ConvertProductToProductDetailDto(product);
        return Result<ProductDetailDto>.Success(productDetailDto);
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

    private async Task<ProductDetailDto> ConvertProductToProductDetailDto(Product product)
    {
        var detailDto = Mapper.Map<Product, ProductDetailDto>(product);

        var imagesResult = await _productImageService.GetAllProductImagesAsync(product.ProductId);
        if (imagesResult.IsSuccess)
            detailDto.Images = imagesResult.Data;

        var productSubcategories = await Dapper.GetByFieldAsync<ProductSubcategory>("productId", product.ProductId);
        var subcategories = await Dapper.GetWhereInAsync<Subcategory>("subcategoryId", productSubcategories.Select(s => s.SubcategoryId).ToList());
        detailDto.Subcategories = subcategories.Select(s => Mapper.Map<Subcategory, SubcategoryDto>(s)).ToList();
        
        return detailDto;
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