using API.Database;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController(IDataContextDapper dapper) : ControllerBase
{
    [HttpGet("all")]
    public List<Product> GetAllProducts()
    {
        return dapper.Query<Product>("SELECT * FROM dbo.product");
    }
    
    
    [HttpGet("{productId}")]
    public Product GetProduct(int productId)
    {
        return dapper.GetById<Product>(productId);
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary)
{
  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
