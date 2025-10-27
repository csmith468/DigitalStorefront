using API.Extensions;
using API.Models.Constants;
using API.Models.Dtos;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("common")]
public class CommonController : ControllerBase
{
    private readonly IProductService _productService;

    public CommonController(IProductService productService)
    {
        _productService = productService;
    }
    
    [HttpGet("product-types")]
    public async Task<ActionResult<List<ProductTypeDto>>> GetProductTypesAsync()
    {
        return (await _productService.GetProductTypesAsync()).ToActionResult();
    }

    [HttpGet("price-types")]
    public async Task<ActionResult<List<PriceType>>> GetPriceTypesAsync()
    {
        var priceTypes = PriceTypes.All;
        return Ok(priceTypes);
    }
}