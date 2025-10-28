using API.Extensions;
using API.Models;
using API.Models.Dtos;
using API.Services;
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
[Route("products")]
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
    public async Task<ActionResult<PaginatedResponse<ProductDto>>> GetProductsAsync([FromQuery] ProductFilterParams filterParams)
    {
        return (await _productService.GetProductsAsync(filterParams)).ToActionResult();
    }
    
    // NOTE: Created dedicated endpoint for a common UI request to get products by category
    [HttpGet("category/{categorySlug}")]
    public async Task<ActionResult<PaginatedResponse<ProductDto>>> GetProductsByCategory(string categorySlug, 
        [FromQuery] PaginationParams pagination)
    {
        var filterParams = new ProductFilterParams
        {
            CategorySlug = categorySlug,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
        return (await _productService.GetProductsAsync(filterParams)).ToActionResult();
    }

    // NOTE: Created dedicated endpoint for a common UI request to get products by subcategory
    [HttpGet("subcategory/{subcategorySlug}")]
    public async Task<ActionResult<PaginatedResponse<ProductDto>>> GetProductsBySubcategory(string subcategorySlug, 
        [FromQuery] PaginationParams pagination)
    {
        var filterParams = new ProductFilterParams
        {
            SubcategorySlug = subcategorySlug,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
        return (await _productService.GetProductsAsync(filterParams)).ToActionResult();
    }
    
    [HttpGet("{productId}")]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(int productId)
    {
        return (await _productService.GetProductByIdAsync(productId)).ToActionResult();
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ProductDetailDto>> GetProductBySlug(string slug)
    {
        return (await _productService.GetProductBySlugAsync(slug)).ToActionResult();
    }

    [Authorize(Policy = "CanManageProducts")]
    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> CreateProduct([FromBody] ProductFormDto dto)
    {
        return (await _productService.CreateProductAsync(dto, _userContext.UserId!.Value)).ToActionResult();
    }

    [Authorize(Policy = "CanManageProducts")]
    [HttpPut("{productId}")]
    public async Task<ActionResult<ProductDetailDto>> UpdateProduct(int productId, [FromBody] ProductFormDto dto)
    {
        return (await _productService.UpdateProductAsync(productId, dto)).ToActionResult();
    }
    
    // Considered [Authorize(Policy = "RequireAdmin")] but will allow users to delete non-demo products they create
    [Authorize(Policy = "CanManageProducts")]
    [HttpDelete("{productId}")]
    public async Task<ActionResult<bool>> DeleteProduct(int productId)
    {
        return (await _productService.DeleteProductAsync(productId)).ToActionResult();
    }
    
    [Authorize(Policy = "CanManageImages")]
    [HttpPost("{productId}/images")]
    public async Task<ActionResult<ProductImageDto>> UploadProductImage(int productId, [FromForm] AddProductImageDto dto)
    {
        return (await _productImageService.AddProductImageAsync(productId, dto)).ToActionResult();
    }

    [Authorize(Policy = "CanManageImages")]
    [HttpDelete("{productId}/images/{productImageId}")]
    public async Task<ActionResult<bool>> DeleteProductImage(int productId, int productImageId)
    {
        return (await _productImageService.DeleteProductImageAsync(productId, productImageId)).ToActionResult();
    }

    [Authorize(Policy = "CanManageImages")]
    [HttpPut("{productId}/images/{productImageId}/set-primary")]
    public async Task<ActionResult<bool>> SetProductImagePrimary(int productId, int productImageId)
    {
        return (await _productImageService.SetPrimaryImageAsync(productId, productImageId)).ToActionResult();
    }

    [Authorize(Policy = "CanManageImages")]
    [HttpPut("{productId}/images/reorder")]
    public async Task<ActionResult<bool>> ReorderProductImages(int productId, List<int> productImageIds)
    {
        return (await _productImageService.ReorderProductImagesAsync(productId, productImageIds)).ToActionResult();
    }
}

