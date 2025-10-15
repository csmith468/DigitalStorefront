using API.Extensions;
using API.Models.Dtos;
using API.Services;
using API.Setup;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("category")]
public class CategoryController(ISharedContainer container) : BaseController(container)
{
    private ICategoryService _categoryService => DepInj<ICategoryService>();
    
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategoriesAndSubcategories()
    {
        return (await _categoryService.GetCategoriesAndSubcategoriesAsync()).ToActionResult();
    }
}