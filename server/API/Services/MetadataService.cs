using API.Database;
using API.Models;
using API.Models.Constants;
using API.Models.DboTables;
using API.Models.Dtos;
using AutoMapper;

namespace API.Services;

public interface IMetadataService
{
    Task<Result<List<CategoryDto>>> GetCategoriesAndSubcategoriesAsync();
    Task<Result<List<ProductTypeDto>>> GetProductTypesAsync();
    List<PriceType> GetPriceTypes();
}

public class MetadataService : IMetadataService
{
    private readonly IQueryExecutor _queryExecutor;
    private  readonly IMapper _mapper;

    public MetadataService(IQueryExecutor queryExecutor, IMapper mapper)
    {
        _queryExecutor = queryExecutor;
        _mapper = mapper;
    }
    
    public async Task<Result<List<CategoryDto>>> GetCategoriesAndSubcategoriesAsync()
    {
        var categories = (await _queryExecutor.QueryAsync<CategoryDto>(
            "SELECT categoryId, [name], slug, displayOrder FROM dbo.category WHERE isActive = 1"
        )).OrderBy(c => c.DisplayOrder).ToList();

        // Looping because there are only 5 categories and this data is cached both in API and UI
        foreach (var category in categories)
        {
            category.Subcategories = (await _queryExecutor.QueryAsync<SubcategoryDto>(
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
    
    public async Task<Result<List<ProductTypeDto>>> GetProductTypesAsync()
    {
        var productTypes = await _queryExecutor.GetAllAsync<ProductType>();
        var productTypeDtos = productTypes.Select(pt => _mapper.Map<ProductTypeDto>(pt)).ToList();
        return Result<List<ProductTypeDto>>.Success(productTypeDtos);
    }

    public List<PriceType> GetPriceTypes()
    {
        return PriceTypes.All;
    }
}