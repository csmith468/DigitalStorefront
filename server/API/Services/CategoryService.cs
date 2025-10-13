using API.Models;
using API.Models.Dtos;
using API.Setup;

namespace API.Services;

public interface ICategoryService
{
    Task<Result<List<CategoryDto>>> GetCategoriesAndSubcategoriesAsync();
}

public class CategoryService(ISharedContainer container) : BaseService(container), ICategoryService
{
    public async Task<Result<List<CategoryDto>>> GetCategoriesAndSubcategoriesAsync()
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
                   AND categoryId = @categoryId
                 """, new { categoryId = category.CategoryId })
                ).OrderBy(s => s.DisplayOrder).ToList();
        }
        return Result<List<CategoryDto>>.Success(categories);
    }
}