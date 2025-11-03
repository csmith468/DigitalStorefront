using API.Extensions;
using API.Models.Constants;
using API.Models.Dtos;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

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
    [OutputCache(PolicyName = "StaticData")]
    public async Task<ActionResult<List<CategoryDto>>> GetCategoriesAndSubcategories()
    {
        return (await _metadataService.GetCategoriesAndSubcategoriesAsync()).ToActionResult();
    }

    [HttpGet("tags")]
    [OutputCache(Tags = ["tags"], Duration = 300)]
    public async Task<ActionResult<List<TagDto>>> GetTags()
    {
        return (await _tagService.GetAllTagsAsync()).ToActionResult();
    }
    
    [HttpGet("product-types")]
    [OutputCache(PolicyName = "StaticData")]
    public async Task<ActionResult<List<ProductTypeDto>>> GetProductTypesAsync()
    {
        return (await _metadataService.GetProductTypesAsync()).ToActionResult();
    }

    [HttpGet("price-types")]
    [OutputCache(PolicyName = "StaticData")]
    public ActionResult<List<PriceType>> GetPriceTypes()
    {
        return _metadataService.GetPriceTypes();
    }
}