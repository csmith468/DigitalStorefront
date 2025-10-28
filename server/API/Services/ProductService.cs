using System.Net;
using API.Database;
using API.Extensions;
using API.Models;
using API.Models.Constants;
using API.Models.DboTables;
using API.Models.Dtos;
using API.Services.Images;
using AutoMapper;
using Dapper;

namespace API.Services;

public interface IProductService
{
    Task<Result<ProductDetailDto>> GetProductByIdAsync(int productId);
    Task<Result<ProductDetailDto>> GetProductBySlugAsync(string slug);
    Task<Result<PaginatedResponse<ProductDto>>> GetProductsAsync(ProductFilterParams filterParams);
    Task<Result<ProductDetailDto>> CreateProductAsync(ProductFormDto dto, int userId);
    Task<Result<ProductDetailDto>> UpdateProductAsync(int productId, ProductFormDto dto);
    Task<Result<bool>> DeleteProductAsync(int productId);
    Task<Result<List<ProductTypeDto>>> GetProductTypesAsync();
}

public class ProductService : IProductService
{
    private readonly IDataContextDapper _dapper;
    private readonly ILogger<ProductService> _logger;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly IUserContext _userContext;
    private readonly IProductImageService _productImageService;
    private readonly IImageStorageService _imageStorageService;
    private readonly IProductAuthorizationService _productAuthService;

    public ProductService(IDataContextDapper dapper,
        ILogger<ProductService> logger,
        IMapper mapper,
        IConfiguration config,
        IUserContext userContext,
        IProductImageService productImageService,
        IImageStorageService imageStorageService,
        IProductAuthorizationService productAuthService)
    {
        _dapper = dapper;
        _logger = logger;
        _mapper = mapper;
        _config = config;
        _userContext = userContext;
        _productImageService = productImageService;
        _imageStorageService = imageStorageService;
        _productAuthService = productAuthService;
    }
    
    public async Task<Result<ProductDetailDto>> GetProductByIdAsync(int productId)
    {
        var product = await _dapper.GetByIdAsync<Product>(productId);
        if (product == null)
            return Result<ProductDetailDto>.Failure($"Product {productId} not found", HttpStatusCode.NotFound);
        
        var productDetailDto = await ConvertProductToProductDetailDto(product);
        return Result<ProductDetailDto>.Success(productDetailDto);
    }

    public async Task<Result<ProductDetailDto>> GetProductBySlugAsync(string slug)
    {
        var product = (await _dapper.GetByFieldAsync<Product>("slug", slug)).FirstOrDefault();
        if (product == null)
            return Result<ProductDetailDto>.Failure("Product could not be found");
        
        var productDetailDto = await ConvertProductToProductDetailDto(product);
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

        var relevanceExpression = "";
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
        var whereClause = whereConditions.Count != 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";

        var baseQuery = $"""
                         SELECT DISTINCT p.* {relevanceExpression}
                         FROM dbo.product p
                         LEFT JOIN dbo.productSubcategory ps ON p.productId = ps.productId
                         LEFT JOIN dbo.subcategory s ON s.subcategoryId = ps.subcategoryId
                         LEFT JOIN dbo.category c ON c.categoryId = s.categoryId
                         {whereClause}
                         """;
        var customOrderBy = !string.IsNullOrWhiteSpace(filterParams.Search)
            ? "Relevance ASC, isDemoProduct DESC, p.productId"
            : null;
        var orderByColumn = string.IsNullOrWhiteSpace(filterParams.Search) ? "productId" : null;
        
        var (products, totalCount) = await _dapper.GetPaginatedWithSqlAsync<Product>(
            baseQuery,
            filterParams,
            parameters,
            orderByColumn: orderByColumn,
            descending: false,
            customOrderBy: customOrderBy
        );
        
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
        if (await _dapper.ExistsByFieldAsync<Product>("name", dto.Name))
            return Result<ProductDetailDto>.Failure($"Product name {dto.Name} already exists", HttpStatusCode.BadRequest);
        if (await _dapper.ExistsByFieldAsync<Product>("slug", dto.Slug))
            return Result<ProductDetailDto>.Failure($"Product slug {dto.Slug} already exists", HttpStatusCode.BadRequest);
        if (dto.PremiumPrice > dto.Price)
            return Result<ProductDetailDto>.Failure("Premium price cannot exceed regular price", HttpStatusCode.BadRequest);

        var product = _mapper.Map<ProductFormDto, Product>(dto);
        product.Slug = product.Slug.ToLower();
        await _dapper.WithTransactionAsync(async () =>
        {
            product.ProductId = await _dapper.InsertAsync(product);
            
            product.Sku = GenerateSku(product.ProductId, product.Slug);
            await _dapper.ExecuteAsync(
                "UPDATE dbo.Product SET sku = @sku WHERE productId = @productId",
                new { sku = product.Sku, product.ProductId } 
            );
            await SetProductSubcategoriesAsync(product.ProductId, dto.SubcategoryIds);
        });

        if (product.ProductId == 0)
            return Result<ProductDetailDto>.Failure("Product could not be created.", HttpStatusCode.InternalServerError);
        
        _logger.LogInformation("Product Created: ProductId {ProductId}, Name {ProductName}, CreatedBy {UserId}",
            product.ProductId, product.Name, userId);
        var productDetailDto = await ConvertProductToProductDetailDto(product);
        return Result<ProductDetailDto>.Success(productDetailDto, HttpStatusCode.Created);
    }

    public async Task<Result<ProductDetailDto>> UpdateProductAsync(int productId, ProductFormDto dto)
    {
        var product = await _dapper.GetByIdAsync<Product>(productId);
        if (product == null)
            return Result<ProductDetailDto>.Failure("Product could not be found");
        
        var manageProductResult = _productAuthService.CanUserManageProduct(product);
        if (!manageProductResult.IsSuccess)
            return manageProductResult.ToFailure<bool, ProductDetailDto>();
        
        if (product.Name != dto.Name && await _dapper.ExistsByFieldAsync<Product>("name", dto.Name))
            return Result<ProductDetailDto>.Failure($"Name {dto.Name} already exists", HttpStatusCode.BadRequest);
        if (product.Slug != dto.Slug && await _dapper.ExistsByFieldAsync<Product>("slug", dto.Slug))
            return Result<ProductDetailDto>.Failure($"Slug {dto.Slug} already exists", HttpStatusCode.BadRequest);
        if (dto.PremiumPrice > dto.Price)
            return Result<ProductDetailDto>.Failure("Premium price cannot exceed regular price", HttpStatusCode.BadRequest);
        
        await _dapper.WithTransactionAsync(async () =>
        {
            _mapper.Map(dto, product);
            await _dapper.UpdateAsync(product);

            await SetProductSubcategoriesAsync(productId, dto.SubcategoryIds);
        });
        
        _logger.LogInformation("Product Modified: ProductId {ProductId}, Name {ProductName}, ModifiedBy {UserId}",
            product.ProductId, product.Name, _userContext.UserId);
        var productDetailDto = await ConvertProductToProductDetailDto(product);
        return Result<ProductDetailDto>.Success(productDetailDto);
    }
    
    public async Task<Result<bool>> DeleteProductAsync(int productId)
    {
        var product = await _dapper.GetByIdAsync<Product>(productId);
        if (product == null)
            return Result<bool>.Failure("Product not found", HttpStatusCode.NotFound);
        
        var manageProductResult = _productAuthService.CanUserManageProduct(product);
        if (!manageProductResult.IsSuccess)
            return manageProductResult.ToFailure<bool, bool>();

        try
        {
            await _dapper.WithTransactionAsync(async () =>
            {
                var images = (await _dapper.GetByFieldAsync<ProductImage>("productId", productId)).ToList();
                if (images.Count != 0)
                {
                    var imageIds = images.Select(i => i.ProductImageId).ToList();
                    await _dapper.DeleteWhereInAsync<ProductImage>("productImageId", imageIds);
                    
                    foreach (var image in images)
                        await _imageStorageService.DeleteImageAsync(image.ImageUrl);
                }
                await _dapper.ExecuteAsync("DELETE FROM dbo.productSubcategory WHERE productId = @productId", new { productId });
                await _dapper.DeleteByIdAsync<Product>(productId);
            });

            _logger.LogInformation("Product Deleted: ProductId {ProductId}, Name {ProductName}, DeletedBy {UserId}",
                product.ProductId, product.Name, _userContext.UserId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete product: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<Result<List<ProductTypeDto>>> GetProductTypesAsync()
    {
        var productTypes = await _dapper.GetAllAsync<ProductType>();
        var productTypeDtos = productTypes.Select(pt => _mapper.Map<ProductTypeDto>(pt)).ToList();
        return Result<List<ProductTypeDto>>.Success(productTypeDtos);
    }
    
    private async Task<bool> CanUserCreateProduct(int userId)
    {
        if (!_config.GetValue<bool>("DemoMode")) return true;
        var userProductCount = await _dapper.GetCountByFieldAsync<Product>("createdBy", userId);
        if (userProductCount <= 3) return true;
        
        _logger.LogWarning("Product creation limit reached: UserId {UserId}", _userContext.UserId);
        return false;
    }
    
    private async Task SetProductSubcategoriesAsync(int productId, List<int> updatedSubcategoriesIds)
    {
        var existingSubcategories = await _dapper.GetByFieldAsync<ProductSubcategory>("productId", productId);
        
        var subcategoryIdsToAdd = updatedSubcategoriesIds.Where(us => 
            !existingSubcategories.Select(es => es.SubcategoryId).Contains(us)).ToList();
        var subcategoriesToRemove = existingSubcategories.Where(es => 
            !updatedSubcategoriesIds.Contains(es.SubcategoryId)).ToList();
        
        foreach (var subcategoryId in subcategoryIdsToAdd)
        {
            await _dapper.ExecuteAsync(
                "INSERT INTO dbo.productSubcategory (productId, subcategoryId) VALUES (@productId, @subcategoryId)",
                new { productId, subcategoryId }
            );
        }

        if (subcategoriesToRemove.Count != 0)
        {
            var idsToRemove = subcategoriesToRemove.Select(s => s.ProductSubcategoryId).ToList();
            await _dapper.DeleteWhereInAsync<ProductSubcategory>("productSubcategoryId", idsToRemove);
        }
    }

    private async Task<ProductDetailDto> ConvertProductToProductDetailDto(Product product)
    {
        var detailDto = _mapper.Map<Product, ProductDetailDto>(product);

        var imagesResult = await _productImageService.GetAllProductImagesAsync(product.ProductId);
        if (imagesResult.IsSuccess)
            detailDto.Images = imagesResult.Data;

        var productSubcategories = await _dapper.GetByFieldAsync<ProductSubcategory>("productId", product.ProductId);
        var subcategories = await _dapper.GetWhereInAsync<Subcategory>("subcategoryId", productSubcategories.Select(s => s.SubcategoryId).ToList());
        detailDto.Subcategories = subcategories.Select(s => _mapper.Map<Subcategory, SubcategoryDto>(s)).ToList();
        
        var priceType = PriceTypes.All.FirstOrDefault(pt => pt.PriceTypeId == product.PriceTypeId);
        detailDto.PriceIcon = priceType != null ? priceType.Icon : "";
        
        return detailDto;
    }

    private async Task<Result<List<ProductDto>>> ConvertProductsToProductDtos(List<Product> products)
    {
        var productIds = products.Select(p => p.ProductId).ToList();
        var primaryImages = await _productImageService.GetPrimaryImagesForProductIds(productIds);

        var productDtos = products.Select(p =>
        {
            var productDto = _mapper.Map<Product, ProductDto>(p);
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
}