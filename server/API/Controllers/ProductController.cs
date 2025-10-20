using API.Extensions;
using API.Models;
using API.Models.Dtos;
using API.Services;
using API.Setup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("product")]
public class ProductController(ISharedContainer container) : BaseController(container)
{
    private IProductService _productService => DepInj<IProductService>();
    private IProductImageService _productImageService => DepInj<IProductImageService>();
    
    [HttpGet("category/{categorySlug}")]
    public async Task<ActionResult<List<ProductDto>>> GetProductsByCategory(string categorySlug)
    {
        return (await _productService.GetProductsByCategoryAsync(categorySlug)).ToActionResult();
    }

    [HttpGet("subcategory/{subcategorySlug}")]
    public async Task<ActionResult<List<ProductDto>>> GetProductsBySubcategory(string subcategorySlug)
    {
        return (await _productService.GetProductsBySubcategoryAsync(subcategorySlug)).ToActionResult();
    }
    
    [HttpGet("{productId}")]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(int productId)
    {
        return (await _productService.GetProductByIdAsync(productId)).ToActionResult();
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> CreateProduct([FromBody] ProductFormDto dto)
    {
        if (UserId == null)
            return Result<ProductDetailDto>.Failure("You are not logged in.").ToActionResult();
        var result = await _productService.CreateProductAsync(dto, UserId.Value);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetProduct), new { productId = result.Data.ProductId }, result.Data)
            : BadRequest(result.Error);
    }

    [Authorize]
    [HttpPut("{productId}")]
    public async Task<ActionResult<ProductDetailDto>> UpdateProduct(int productId, [FromBody] ProductFormDto dto)
    {
        if (UserId == null)
            return Result<ProductDetailDto>.Failure("You are not logged in.").ToActionResult();
        return (await _productService.UpdateProductAsync(productId, dto, UserId.Value)).ToActionResult();
    }
    
    [Authorize]
    [HttpPost("{productId}/image")]
    public async Task<ActionResult<ProductImageDto>> UploadProductImage(int productId, [FromForm] AddProductImageDto dto)
    {
        return (await _productImageService.AddProductImageAsync(productId, dto)).ToActionResult();
    }

    [Authorize]
    [HttpDelete("{productId}/image/{productImageId}")]
    public async Task<ActionResult<bool>> DeleteProductImage(int productId, int productImageId)
    {
        return (await _productImageService.DeleteProductImageAsync(productId, productImageId)).ToActionResult();
    }

    [Authorize]
    [HttpPut("{productId}/image/{productImageId}/set-primary")]
    public async Task<ActionResult<bool>> SetProductImagePrimary(int productId, int productImageId)
    {
        return (await _productImageService.SetPrimaryImageAsync(productId, productImageId)).ToActionResult();
    }
}

