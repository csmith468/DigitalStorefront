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
    [HttpPost("create")]
    public async Task<ActionResult<int>> CreateProduct([FromBody] ProductFormDto dto)
    {
        if (UserId == null)
            return Result<int>.Failure("You are not logged in.").ToActionResult();

        return (await _productService.CreateProductAsync(dto, UserId.Value)).ToActionResult();
    }

    [Authorize]
    [HttpPut("update/{productId}")]
    public async Task<ActionResult<bool>> UpdateProduct(int productId, [FromBody] ProductFormDto dto)
    {
        if (UserId == null)
            return Result<bool>.Failure("You are not logged in.").ToActionResult();
        return (await _productService.UpdateProductAsync(productId, dto, UserId.Value)).ToActionResult();
    }
}

