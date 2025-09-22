using API.Database;
using API.Models;
using API.Setup;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController(ISharedContainer container) : BaseController(container)
{
    [HttpGet("all")]
    public List<Product> GetAllProducts()
    {
        return Dapper.Query<Product>("SELECT * FROM dbo.product");
    }
    
    
    [HttpGet("{productId}")]
    public Product GetProduct(int productId)
    {
        return Dapper.GetById<Product>(productId);
    }
}

