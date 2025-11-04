using API.Extensions;
using API.Models.Dtos;
using API.Services;
using API.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Product CRUD endpoints with role-based authorization
///
/// Roles:
/// - Admin: Can manage all products including demo products
/// - ProductWriter: Can manage non-demo products
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IProductImageService _productImageService;
    private readonly IUserContext _userContext;

    public ProductsController(IProductService productService, IProductImageService productImageService, IUserContext userContext)
    {
        _productService = productService;
        _productImageService = productImageService;
        _userContext = userContext;
    }
    
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ProductDto>>> GetProductsAsync([FromQuery] ProductFilterParams filterParams, CancellationToken ct)
    {
        return (await _productService.GetProductsAsync(filterParams, ct)).ToActionResult();
    }
    
    // NOTE: Created dedicated endpoint for a common UI request to get products by category
    [HttpGet("category/{categorySlug}")]
    public async Task<ActionResult<PaginatedResponse<ProductDto>>> GetProductsByCategoryAsync(string categorySlug,
        [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var filterParams = new ProductFilterParams
        {
            CategorySlug = categorySlug,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
        return (await _productService.GetProductsAsync(filterParams, ct)).ToActionResult();
    }

    // NOTE: Created dedicated endpoint for a common UI request to get products by subcategory
    [HttpGet("subcategory/{subcategorySlug}")]
    public async Task<ActionResult<PaginatedResponse<ProductDto>>> GetProductsBySubcategoryAsync(string subcategorySlug,
        [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var filterParams = new ProductFilterParams
        {
            SubcategorySlug = subcategorySlug,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
        return (await _productService.GetProductsAsync(filterParams, ct)).ToActionResult();
    }
    
    [HttpGet("{productId}")]
    public async Task<ActionResult<ProductDetailDto>> GetProductAsync(int productId, CancellationToken ct)
    {
        return (await _productService.GetProductByIdAsync(productId, ct)).ToActionResult();
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ProductDetailDto>> GetProductBySlugAsync(string slug, CancellationToken ct)
    {
        return (await _productService.GetProductBySlugAsync(slug, ct)).ToActionResult();
    }

    [Authorize(Policy = "CanManageProducts")]
    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> CreateProductAsync([FromBody] ProductFormDto dto, CancellationToken ct)
    {
        return (await _productService.CreateProductAsync(dto, _userContext.UserId!.Value, ct)).ToActionResult();
    }

    [Authorize(Policy = "CanManageProducts")]
    [HttpPut("{productId}")]
    public async Task<ActionResult<ProductDetailDto>> UpdateProductAsync(int productId, [FromBody] ProductFormDto dto, CancellationToken ct)
    {
        return (await _productService.UpdateProductAsync(productId, dto, ct)).ToActionResult();
    }

    // Considered [Authorize(Policy = "RequireAdmin")] but will allow users to delete non-demo products they create
    [Authorize(Policy = "CanManageProducts")]
    [HttpDelete("{productId}")]
    public async Task<ActionResult<bool>> DeleteProductAsync(int productId, CancellationToken ct)
    {
        return (await _productService.DeleteProductAsync(productId, ct)).ToActionResult();
    }
    
    [Authorize(Policy = "CanManageImages")]
    [HttpPost("{productId}/images")]
    public async Task<ActionResult<ProductImageDto>> UploadProductImageAsync(int productId, [FromForm] AddProductImageDto dto, CancellationToken ct)
    {
        return (await _productImageService.AddProductImageAsync(productId, dto, ct)).ToActionResult();
    }

    [Authorize(Policy = "CanManageImages")]
    [HttpDelete("{productId}/images/{productImageId}")]
    public async Task<ActionResult<bool>> DeleteProductImageAsync(int productId, int productImageId, CancellationToken ct)
    {
        return (await _productImageService.DeleteProductImageAsync(productId, productImageId, ct)).ToActionResult();
    }

    [Authorize(Policy = "CanManageImages")]
    [HttpPut("{productId}/images/{productImageId}/set-primary")]
    public async Task<ActionResult<bool>> SetProductImagePrimaryAsync(int productId, int productImageId, CancellationToken ct)
    {
        return (await _productImageService.SetPrimaryImageAsync(productId, productImageId, ct)).ToActionResult();
    }

    [Authorize(Policy = "CanManageImages")]
    [HttpPut("{productId}/images/reorder")]
    public async Task<ActionResult<bool>> ReorderProductImagesAsync(int productId, List<int> productImageIds, CancellationToken ct)
    {
        return (await _productImageService.ReorderProductImagesAsync(productId, productImageIds, ct)).ToActionResult();
    }
}

