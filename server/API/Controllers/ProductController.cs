using API.Models.DboTables;
using API.Setup;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("product")]
public class ProductController(ISharedContainer container) : BaseController(container)
{
    [HttpGet("all")]
    public async Task<ActionResult<List<Product>>> GetAllProducts()
    {
        return (await Dapper.GetAllAsync<Product>()).ToList();
    }

    [HttpGet("subcategory/{subcategorySlug}")]
    public async Task<ActionResult<List<Product>>> GetProductsBySubcategory(string subcategorySlug)
    {
        var categoryExists = await Dapper.ExistsByFieldAsync<Subcategory>("slug", subcategorySlug);
        if (!categoryExists) return NotFound($"Subcategory slug {subcategorySlug} not found.");
        
        var products = (await Dapper.QueryAsync<Product>(
            """
                SELECT p.*
                FROM dbo.product p
                JOIN dbo.productSubcategory ps ON p.productId = ps.productId
                join dbo.subcategory s ON s.subcategoryId = ps.subcategoryId
                WHERE s.slug = @subcategorySlug
            """,
            new { subcategorySlug })).ToList();
        
        if (products.Count == 0) return NotFound($"No products found for {subcategorySlug}.");
        return Ok(products);
    }
    
    
    [HttpGet("{productId}")]
    public async Task<ActionResult<Product>> GetProduct(int productId)
    {
        var product = await Dapper.GetByIdAsync<Product>(productId);
        if (product == null) return NotFound($"Product ID {productId} not found.");
        return Ok(product);
    }
}

