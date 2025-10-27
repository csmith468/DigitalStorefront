using API.Extensions;
using API.Models.Dtos;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace API.Controllers;

[ApiController]
[Route("category")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    
    [HttpGet]
    [OutputCache(PolicyName = "StaticData")]
    public async Task<ActionResult<List<CategoryDto>>> GetCategoriesAndSubcategories()
    {
        return (await _categoryService.GetCategoriesAndSubcategoriesAsync()).ToActionResult();
    }
}