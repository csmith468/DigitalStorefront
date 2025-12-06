using API.Database;
using API.Models;
using API.Models.Constants;
using API.Models.DboTables;
using API.Models.Dtos;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;

namespace API.Services;

public interface IMetadataService
{
    Task<Result<List<CategoryDto>>> GetCategoriesAndSubcategoriesAsync(CancellationToken ct = default);
    Task<Result<List<ProductTypeDto>>> GetProductTypesAsync(CancellationToken ct = default);
    List<PriceType> GetPriceTypes();
}

public class MetadataService : IMetadataService
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);

    private const string CategoriesCacheKey = "metadata:categories";
    private const string ProductTypesCacheKey = "metadata:productTypes";

    public MetadataService(IQueryExecutor queryExecutor, IMapper mapper, IMemoryCache cache)
    {
        _queryExecutor = queryExecutor;
        _mapper = mapper;
        _cache = cache;
    }
    
    public async Task<Result<List<CategoryDto>>> GetCategoriesAndSubcategoriesAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CategoriesCacheKey, out List<CategoryDto>? cached))
            return Result<List<CategoryDto>>.Success(cached!);
        
        var categories = (await _queryExecutor.QueryAsync<CategoryDto>(
            "SELECT categoryId, [name], slug, displayOrder FROM dbo.category WHERE isActive = 1", null, ct
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
                 """, new { categoryId = category.CategoryId }, ct)
                ).OrderBy(s => s.DisplayOrder).ToList();
        }

        _cache.Set(CategoriesCacheKey, categories, _cacheDuration);
        return Result<List<CategoryDto>>.Success(categories);
    }
    
    public async Task<Result<List<ProductTypeDto>>> GetProductTypesAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(ProductTypesCacheKey, out List<ProductTypeDto>? cached))
            return Result<List<ProductTypeDto>>.Success(cached!);
        
        var productTypes = await _queryExecutor.GetAllAsync<ProductType>(ct);
        var productTypeDtos = productTypes.Select(pt => _mapper.Map<ProductTypeDto>(pt)).ToList();
        
        _cache.Set(ProductTypesCacheKey, productTypeDtos, _cacheDuration);
        return Result<List<ProductTypeDto>>.Success(productTypeDtos);
    }

    public List<PriceType> GetPriceTypes()
    {
        return PriceTypes.All;
    }
}