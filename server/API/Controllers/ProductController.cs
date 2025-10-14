using API.Extensions;
using API.Models.DboTables;
using API.Services;
using API.Setup;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("product")]
public class ProductController(ISharedContainer container) : BaseController(container)
{
    private IProductService _productService => DepInj<IProductService>();
    
    [HttpGet("all")]
    public async Task<ActionResult<List<Product>>> GetAllProducts()
    {
        return (await _productService.GetAllProductsAsync()).ToActionResult();
    }

    [HttpGet("subcategory/{subcategorySlug}")]
    public async Task<ActionResult<List<Product>>> GetProductsBySubcategory(string subcategorySlug)
    {
        return (await _productService.GetProductsBySubcategoryAsync(subcategorySlug)).ToActionResult();
    }
    
    [HttpGet("{productId}")]
    public async Task<ActionResult<Product>> GetProduct(int productId)
    {
        return (await _productService.GetProductByIdAsync(productId)).ToActionResult();
    }
}

