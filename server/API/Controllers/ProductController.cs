using API.Extensions;
using API.Models.DboTables;
using API.Models.Dtos;
using API.Services;
using API.Setup;
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
}

