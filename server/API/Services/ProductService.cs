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
}

public class ProductService : IProductService
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<ProductService> _logger;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly IUserContext _userContext;
    private readonly IProductImageService _productImageService;
    private readonly IImageStorageService _imageStorageService;
    private readonly IProductAuthorizationService _productAuthService;
    private readonly ITagService _tagService;

    public ProductService(
        IQueryExecutor queryExecutor,
        ICommandExecutor commandExecutor,
        ITransactionManager transactionManager,
        ILogger<ProductService> logger,
        IMapper mapper,
        IConfiguration config,
        IUserContext userContext,
        IProductImageService productImageService,
        IImageStorageService imageStorageService,
        IProductAuthorizationService productAuthService,
        ITagService tagService)
    {
        _queryExecutor = queryExecutor;
        _commandExecutor = commandExecutor;
        _transactionManager = transactionManager;
        _logger = logger;
        _mapper = mapper;
        _config = config;
        _userContext = userContext;
        _productImageService = productImageService;
        _imageStorageService = imageStorageService;
        _productAuthService = productAuthService;
        _tagService = tagService;
    }
    
    public async Task<Result<ProductDetailDto>> GetProductByIdAsync(int productId)
    {
        var product = await _queryExecutor.GetByIdAsync<Product>(productId);
        if (product == null)
            return Result<ProductDetailDto>.Failure($"Product {productId} not found");
        
        var productDetailDto = await ConvertProductToProductDetailDto(product);
        return Result<ProductDetailDto>.Success(productDetailDto);
    }

    public async Task<Result<ProductDetailDto>> GetProductBySlugAsync(string slug)
    {
        var product = (await _queryExecutor.GetByFieldAsync<Product>("slug", slug)).FirstOrDefault();
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
            var searchCondition = string.Join(" OR ", 
                "p.name LIKE @search", "p.slug LIKE @search", 
                "c.name LIKE @search", "c.slug LIKE @search", 
                "s.name LIKE @search", "s.slug LIKE @search", 
                "t.name LIKE @search");

            whereConditions.Add(searchCondition);
            parameters.Add("search", $"%{filterParams.Search}%");
            
            relevanceExpression = """
                                  , CASE
                                    WHEN p.name LIKE @search OR p.slug LIKE @search THEN 1
                                    WHEN t.name LIKE @search THEN 2
                                    WHEN s.name LIKE @search OR s.slug LIKE @search THEN 3
                                    WHEN c.name LIKE @search OR c.slug LIKE @search THEN 4
                                    ELSE 5
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
                         LEFT JOIN dbo.productTag pt ON p.productId = pt.productId
                         LEFT JOIN dbo.tag t ON t.tagId = pt.tagId
                         {whereClause}
                         """;
        var customOrderBy = !string.IsNullOrWhiteSpace(filterParams.Search)
            ? new TrustedOrderByExpression("Relevance ASC, isDemoProduct DESC, p.productId")
            : null;
        var orderByColumn = string.IsNullOrWhiteSpace(filterParams.Search) ? "productId" : null;
        
        var (products, totalCount) = await _queryExecutor.GetPaginatedWithSqlAsync<Product>(
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
        
        var validateProductResult = await ValidateProductAsync(dto);
        if (!validateProductResult.IsSuccess)
            return validateProductResult.ToFailure<bool, ProductDetailDto>();

        var product = _mapper.Map<ProductFormDto, Product>(dto);
        product.Slug = product.Slug.ToLower();
        await _transactionManager.WithTransactionAsync(async () =>
        {
            product.ProductId = await _commandExecutor.InsertAsync(product);
            
            product.Sku = GenerateSku(product.ProductId, product.Slug);
            await _commandExecutor.UpdateFieldAsync<Product>(product.ProductId, "sku", product.Sku);
            
            await SetProductSubcategoriesAsync(product.ProductId, dto.SubcategoryIds);

            var tagIds = await _tagService.GetOrCreateTagsAsync(dto.Tags);
            await SetProductTagsAsync(product.ProductId, tagIds);
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
        var product = await _queryExecutor.GetByIdAsync<Product>(productId);
        if (product == null)
            return Result<ProductDetailDto>.Failure("Product could not be found");
        
        var manageProductResult = _productAuthService.CanUserManageProduct(product);
        if (!manageProductResult.IsSuccess)
            return manageProductResult.ToFailure<bool, ProductDetailDto>();
        
        var validateProductResult = await ValidateProductAsync(dto, product);
        if (!validateProductResult.IsSuccess)
            return validateProductResult.ToFailure<bool, ProductDetailDto>();
        
        await _transactionManager.WithTransactionAsync(async () =>
        {
            _mapper.Map(dto, product);
            await _commandExecutor.UpdateAsync(product);

            await SetProductSubcategoriesAsync(productId, dto.SubcategoryIds);
            
            var tagIds = await _tagService.GetOrCreateTagsAsync(dto.Tags);
            await SetProductTagsAsync(product.ProductId, tagIds);
        });
        
        _logger.LogInformation("Product Modified: ProductId {ProductId}, Name {ProductName}, ModifiedBy {UserId}",
            product.ProductId, product.Name, _userContext.UserId);
        var productDetailDto = await ConvertProductToProductDetailDto(product);
        return Result<ProductDetailDto>.Success(productDetailDto);
    }
    
    public async Task<Result<bool>> DeleteProductAsync(int productId)
    {
        var product = await _queryExecutor.GetByIdAsync<Product>(productId);
        if (product == null)
            return Result<bool>.Failure("Product not found");
        
        var manageProductResult = _productAuthService.CanUserManageProduct(product);
        if (!manageProductResult.IsSuccess)
            return manageProductResult.ToFailure<bool, bool>();

        try
        {
            await _transactionManager.WithTransactionAsync(async () =>
            {
                var images = (await _queryExecutor.GetByFieldAsync<ProductImage>("productId", productId)).ToList();
                if (images.Count != 0)
                {
                    var imageIds = images.Select(i => i.ProductImageId).ToList();
                    await _commandExecutor.DeleteWhereInAsync<ProductImage>("productImageId", imageIds);
                    
                    foreach (var image in images)
                        await _imageStorageService.DeleteImageAsync(image.ImageUrl);
                }
                await _commandExecutor.DeleteByFieldAsync<ProductSubcategory>("productId", productId);
                await _commandExecutor.DeleteByFieldAsync<ProductTag>("productId", productId);
                await _commandExecutor.DeleteByIdAsync<Product>(productId);
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
    
    private async Task<bool> CanUserCreateProduct(int userId)
    {
        if (!_config.GetValue<bool>("DemoMode")) return true;
        var userProductCount = await _queryExecutor.GetCountByFieldAsync<Product>("createdBy", userId);
        if (userProductCount <= 3) return true;
        
        _logger.LogWarning("Product creation limit reached: UserId {UserId}", _userContext.UserId);
        return false;
    }
    
    private async Task SetProductSubcategoriesAsync(int productId, List<int> updatedSubcategoriesIds)
    {
        var existingSubcategories = await _queryExecutor.GetByFieldAsync<ProductSubcategory>("productId", productId);
        
        var subcategoryIdsToAdd = updatedSubcategoriesIds.Where(us => 
            !existingSubcategories.Select(es => es.SubcategoryId).Contains(us)).ToList();
        var subcategoriesToRemove = existingSubcategories.Where(es => 
            !updatedSubcategoriesIds.Contains(es.SubcategoryId)).ToList();

        if (subcategoryIdsToAdd.Count > 0)
        {
            var subcategoriesToAdd = subcategoryIdsToAdd.Select(s => new ProductSubcategory { ProductId = productId, SubcategoryId = s });
            await _commandExecutor.BulkInsertAsync(subcategoriesToAdd);
        }

        if (subcategoriesToRemove.Count != 0)
        {
            var idsToRemove = subcategoriesToRemove.Select(s => s.ProductSubcategoryId).ToList();
            await _commandExecutor.DeleteWhereInAsync<ProductSubcategory>("productSubcategoryId", idsToRemove);
        }
    }

    private async Task<ProductDetailDto> ConvertProductToProductDetailDto(Product product)
    {
        var detailDto = _mapper.Map<Product, ProductDetailDto>(product);

        var imagesResult = await _productImageService.GetAllProductImagesAsync(product.ProductId);
        if (imagesResult.IsSuccess)
            detailDto.Images = imagesResult.Data;

        var productSubcategories = await _queryExecutor.GetByFieldAsync<ProductSubcategory>("productId", product.ProductId);
        var subcategories = await _queryExecutor.GetWhereInAsync<Subcategory>("subcategoryId", productSubcategories.Select(s => s.SubcategoryId).ToList());
        detailDto.Subcategories = subcategories.Select(s => _mapper.Map<Subcategory, SubcategoryDto>(s)).ToList();
        
        var productTags = (await _queryExecutor.GetByFieldAsync<ProductTag>("productId", product.ProductId)).ToList();
        if (productTags.Count != 0)
        {
            var tags = await _queryExecutor.GetWhereInAsync<Tag>("tagId", productTags.Select(pt => pt.TagId).ToList());
            detailDto.Tags = tags.Select(t => _mapper.Map<Tag, TagDto>(t)).ToList();
        }
        
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

    private async Task<Result<bool>> ValidateProductAsync(ProductFormDto dto, Product? originalProduct = null)
    {
        if (await _queryExecutor.ExistsByFieldAsync<Product>("name", dto.Name) && (originalProduct == null || originalProduct.Name != dto.Name))
            return Result<bool>.Failure($"Product name {dto.Name} already exists", HttpStatusCode.BadRequest);
        if (await _queryExecutor.ExistsByFieldAsync<Product>("slug", dto.Slug) && (originalProduct == null || originalProduct.Slug != dto.Slug))
            return Result<bool>.Failure($"Product slug {dto.Slug} already exists", HttpStatusCode.BadRequest);
        if (dto.PremiumPrice > dto.Price)
            return Result<bool>.Failure("Premium price cannot exceed regular price", HttpStatusCode.BadRequest);
        if (dto.SubcategoryIds.Count != 0)
        {
            var subcategoryValidationResult = await ValidateSubcategoryIdsAsync(dto.SubcategoryIds);
            if (!subcategoryValidationResult.IsSuccess)
                return subcategoryValidationResult;
        }
        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> ValidateSubcategoryIdsAsync(List<int> subcategoriesIds)
    {
        if (subcategoriesIds.Count == 0)
            return Result<bool>.Success(true);
        
        var distinctIds = subcategoriesIds.Distinct().ToList();
        var existingSubcategories =
            (await _queryExecutor.GetWhereInAsync<Subcategory>("subcategoryId", distinctIds)).ToList();

        if (existingSubcategories.Count == distinctIds.Count) 
            return Result<bool>.Success(true);
        
        var existingIds = existingSubcategories.Select(s => s.SubcategoryId).ToHashSet();
        var nonexistentIds = string.Join(", ", distinctIds.Where(id => !existingIds.Contains(id)).ToList());
        _logger.LogWarning("Invalid subcategoryIds attempted: {NonexistentIds}", nonexistentIds);
        return Result<bool>.Failure($"Invalid subcategoryIds: {nonexistentIds}", HttpStatusCode.BadRequest);
    }

    
    private async Task SetProductTagsAsync(int productId, List<int> updatedTagIds)
    {
        var existingTags = await _queryExecutor.GetByFieldAsync<ProductTag>("productId", productId);

        var tagIdsToAdd = updatedTagIds.Where(ut => !existingTags.Select(et => et.TagId).Contains(ut)).ToList();
        var tagsToRemove = existingTags.Where(et => !updatedTagIds.Contains(et.TagId)).ToList();

        if (tagIdsToAdd.Count > 0)
        {
            var productTagsToAdd = tagIdsToAdd.Select(t => new ProductTag { ProductId = productId, TagId = t });
            await _commandExecutor.BulkInsertAsync(productTagsToAdd);
        }

        if (tagsToRemove.Count != 0)
        {
            var idsToRemove = tagsToRemove.Select(t => t.ProductTagId).ToList();
            await _commandExecutor.DeleteWhereInAsync<ProductTag>("productTagId", idsToRemove);
        }
    }
}