using API.Extensions;
using API.Models.Constants;
using API.Models.Dtos;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetadataController : ControllerBase
{
    private readonly IMetadataService _metadataService;
    private readonly ITagService _tagService;

    public MetadataController(IMetadataService metadataService, ITagService tagService)
    {
        _metadataService = metadataService;
        _tagService = tagService;
    }
    
    [HttpGet("categories")]
    [EnableRateLimiting("anonymous")]
    [OutputCache(PolicyName = "StaticData")]
    public async Task<ActionResult<List<CategoryDto>>> GetCategoriesAndSubcategoriesAsync(CancellationToken ct)
    {
        return (await _metadataService.GetCategoriesAndSubcategoriesAsync(ct)).ToActionResult();
    }

    [HttpGet("tags")]
    [EnableRateLimiting("anonymous")]
    [OutputCache(Tags = ["tags"], Duration = 300)]
    public async Task<ActionResult<List<TagDto>>> GetTagsAsync(CancellationToken ct)
    {
        return (await _tagService.GetAllTagsAsync(ct)).ToActionResult();
    }

    [HttpGet("product-types")]
    [EnableRateLimiting("anonymous")]
    [OutputCache(PolicyName = "StaticData")]
    public async Task<ActionResult<List<ProductTypeDto>>> GetProductTypesAsync(CancellationToken ct)
    {
        return (await _metadataService.GetProductTypesAsync(ct)).ToActionResult();
    }

    [HttpGet("price-types")]
    [EnableRateLimiting("anonymous")]
    [OutputCache(PolicyName = "StaticData")]
    public ActionResult<List<PriceType>> GetPriceTypes()
    {
        return _metadataService.GetPriceTypes();
    }
}