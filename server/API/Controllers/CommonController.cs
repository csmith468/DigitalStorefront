using API.Extensions;
using API.Models.Constants;
using API.Models.Dtos;
using API.Services;
using API.Setup;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("common")]
public class CommonController(ISharedContainer container) : BaseController(container)
{
    private ICommonService _commonService => DepInj<ICommonService>();
    
    [HttpGet("product-types")]
    public async Task<ActionResult<List<ProductTypeDto>>> GetProductTypesAsync()
    {
        return (await _commonService.GetProductTypesAsync()).ToActionResult();
    }

    [HttpGet("price-types")]
    public async Task<ActionResult<List<PriceType>>> GetPriceTypesAsync()
    {
        var priceTypes = PriceTypes.All;
        return Ok(priceTypes);
    }
}