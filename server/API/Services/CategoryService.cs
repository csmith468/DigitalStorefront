using API.Database;
using API.Models;
using API.Models.Dtos;

namespace API.Services;

public interface ICategoryService
{
    Task<Result<List<CategoryDto>>> GetCategoriesAndSubcategoriesAsync();
}

public class CategoryService : ICategoryService
{
    private readonly IDataContextDapper _dapper;
    public CategoryService(IDataContextDapper dapper)
    {
        _dapper = dapper;
    }
    
    public async Task<Result<List<CategoryDto>>> GetCategoriesAndSubcategoriesAsync()
    {
        var categories = (await _dapper.QueryAsync<CategoryDto>(
            "SELECT categoryId, [name], slug, displayOrder FROM dbo.category WHERE isActive = 1"
        )).OrderBy(c => c.DisplayOrder).ToList();

        foreach (var category in categories)
        {
            category.Subcategories = (await _dapper.QueryAsync<SubcategoryDto>(
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