using System.Net;
using API.Extensions;
using API.Models;
using API.Models.Constants;
using API.Models.DboTables;
using API.Models.Dtos;
using API.Services.Images;
using API.Setup;
using Dapper;

namespace API.Services;

public interface IProductService
{
    Task<Result<ProductDetailDto>> GetProductByIdAsync(int productId);
    Task<Result<PaginatedResponse<ProductDto>>> GetProductsAsync(ProductFilterParams filterParams);
    Task<Result<ProductDetailDto>> CreateProductAsync(ProductFormDto dto, int userId);
    Task<Result<ProductDetailDto>> UpdateProductAsync(int productId, ProductFormDto dto);
    Task<Result<bool>> DeleteProductAsync(int productId);
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

    public async Task<Result<PaginatedResponse<ProductDto>>> GetProductsAsync(ProductFilterParams filterParams)
    {
        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        if (filterParams.ProductTypeId.HasValue)
        {
            whereConditions.Add("p.productTypeId = @productTypeId");
            parameters.Add("productTypeId", filterParams.ProductTypeId.Value);
        }
        if (filterParams.CategorySlug != null)
        {
            whereConditions.Add("c.slug = @categorySlug");
            parameters.Add("categorySlug", filterParams.CategorySlug);
        }
        if (filterParams.SubcategorySlug != null)
        {
            whereConditions.Add("s.slug = @subcategorySlug");
            parameters.Add("subcategorySlug", filterParams.SubcategorySlug);
        }

        string relevanceExpression = "";
        if (!string.IsNullOrWhiteSpace(filterParams.Search))
        {
            var prefixes = new[] { "p.", "c.", "s." };
            var searchCondition = "(" + string.Join(" OR ", 
                prefixes.SelectMany(p => new[] { $"{p}name LIKE @search", $"{p}slug LIKE @search" })
            ) + ")";

            whereConditions.Add(searchCondition);
            parameters.Add("search", $"%{filterParams.Search}%");
            
            relevanceExpression = """
                                  , CASE
                                    WHEN p.name LIKE @search OR p.slug LIKE @search THEN 1
                                    WHEN s.name LIKE @search OR s.slug LIKE @search THEN 2
                                    WHEN c.name LIKE @search OR c.slug LIKE @search THEN 3
                                    ELSE 4
                                  END AS Relevance
                                  """;
        }
        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";

        var baseQuery = $"""
                         SELECT DISTINCT p.* {relevanceExpression}
                         FROM dbo.product p
                         LEFT JOIN dbo.productSubcategory ps ON p.productId = ps.productId
                         LEFT JOIN dbo.subcategory s ON s.subcategoryId = ps.subcategoryId
                         LEFT JOIN dbo.category c ON c.categoryId = s.categoryId
                         {whereClause}
                         """;
        var orderBy = !string.IsNullOrWhiteSpace(filterParams.Search)
            ? "Relevance ASC, isDemoProduct DESC, p.productId"
            : "isDemoProduct DESC, p.productId";
        
        var (products, totalCount) = await Dapper.GetPaginatedWithSqlAsync<Product>(baseQuery, filterParams, parameters, orderBy);
        
        var productDtosResult = await ConvertProductsToProductDtos(products.ToList());
        return Result<PaginatedResponse<ProductDto>>.Success(new PaginatedResponse<ProductDto>
        {
            Items = productDtosResult.Data,
            TotalCount = totalCount,
            Page = filterParams.Page,
            PageSize = filterParams.PageSize,
        });
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

    public async Task<Result<ProductDetailDto>> UpdateProductAsync(int productId, ProductFormDto dto)
    {
        var product = await Dapper.GetByIdAsync<Product>(productId);
        if (product == null)
            return Result<ProductDetailDto>.Failure("Product could not be found");
        if (product.IsDemoProduct && Config.GetValue<bool>("DemoMode"))
            return Result<ProductDetailDto>.Failure("Demo products cannot be updated", HttpStatusCode.Unauthorized);
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
    
    public async Task<Result<bool>> DeleteProductAsync(int productId)
    {
        var product = await Dapper.GetByIdAsync<Product>(productId);
        if (product == null)
            return Result<bool>.Failure("Product not found", HttpStatusCode.NotFound);

        if (product.IsDemoProduct)
            return Result<bool>.Failure("Demo products cannot be deleted", HttpStatusCode.Forbidden);

        try
        {
            await Dapper.WithTransactionAsync(async () =>
            {
                var images = await Dapper.GetByFieldAsync<ProductImage>("productId", productId);
                foreach (var image in images)
                {
                    await Dapper.DeleteByIdAsync<ProductImage>(image.ProductImageId);
                    await DepInj<IImageStorageService>().DeleteImageAsync(image.ImageUrl);
                }
                await Dapper.ExecuteAsync("DELETE FROM dbo.productSubcategory WHERE productId = @productId", new { productId });
                await Dapper.DeleteByIdAsync<Product>(productId);
            });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete product: {ex.Message}", HttpStatusCode.InternalServerError);
        }
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