using API.Setup;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("category")]
public class CategoryController(ISharedContainer container) : BaseController(container)
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetMenu()
    {
        var categories = (await Dapper.QueryAsync<CategoryDto>(
            "SELECT categoryId, [name], slug, displayOrder FROM dbo.category WHERE isActive = 1"
            )).OrderBy(c => c.DisplayOrder).ToList();

        foreach (var category in categories)
        {
            category.Subcategories = (await Dapper.QueryAsync<SubcategoryDto>(
                $"""
                 SELECT subcategoryId, [name], slug, displayOrder, imageUrl 
                 FROM dbo.subcategory 
                 WHERE isActive = 1 
                   AND categoryId = {category.CategoryId}
                 """)).OrderBy(s => s.DisplayOrder).ToList();
        }
        return Ok(categories);
    }


    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public int DisplayOrder { get; set; } = 0;
        public List<SubcategoryDto> Subcategories { get; set; } = [];
    }

    public class SubcategoryDto
    {
        public int SubcategoryId { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public int DisplayOrder { get; set; } = 0;
        public string? ImageUrl { get; set; }
    }
}